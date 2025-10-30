using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );
});

// CORS for frontend (restrict to allowed origins from config)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", p =>
    {
        if (allowedOrigins.Length > 0)
        {
            p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // fallback for dev misconfig
        }
    });
});

// DbContext (SQLite) and Identity helpers
builder.Services.AddDbContext<ProjectManagerAPI.Data.AppDbContext>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<ProjectManagerAPI.Models.User>, Microsoft.AspNetCore.Identity.PasswordHasher<ProjectManagerAPI.Models.User>>();

// JWT Auth
var isDev = builder.Environment.IsDevelopment();
var jwtSecret = builder.Configuration["Jwt:Secret"]; // do NOT commit secrets; set via env or user-secrets
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    if (isDev)
    {
        jwtSecret = "dev-secret-change-me"; // fallback only for Development
    }
    else
    {
        throw new InvalidOperationException("Missing configuration: Jwt:Secret must be set in production.");
    }
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "pm.local",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "pm.local",
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret!))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectManagerAPI v1");
    });
}
else
{
    app.UseHttpsRedirection();
}
app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Ensure database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProjectManagerAPI.Data.AppDbContext>();
    db.Database.EnsureCreated();

    // Lightweight dev-time schema patch: add DueDate to Tasks if missing
    try
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(Tasks);";
        using var reader = cmd.ExecuteReader();
        var hasDueDate = false;
        while (reader.Read())
        {
            var colName = reader[1]?.ToString(); // name column
            if (string.Equals(colName, "DueDate", StringComparison.OrdinalIgnoreCase)) { hasDueDate = true; break; }
        }
        reader.Close();
        if (!hasDueDate)
        {
            using var alter = conn.CreateCommand();
            alter.CommandText = "ALTER TABLE Tasks ADD COLUMN DueDate TEXT NULL;";
            alter.ExecuteNonQuery();
        }
    }
    catch
    {
        // Ignore; if migration fails, normal EF queries will surface issues
    }
}

// Map endpoints through minimal APIs (controllers not used to keep concise)
ProjectManagerAPI.Endpoints.AuthEndpoints.Map(app);
ProjectManagerAPI.Endpoints.ProjectEndpoints.Map(app);
ProjectManagerAPI.Endpoints.ScheduleEndpoints.Map(app);

// Health endpoint
app.MapHealthChecks("/health");

// Friendly homepage
app.MapGet("/", () => Results.Json(new
{
    name = "ProjectManagerAPI",
    openapi = "/swagger/v1/swagger.json",
    endpoints = new object[]
    {
        new { method = "POST", path = "/api/auth/register" },
        new { method = "POST", path = "/api/auth/login" },
        new { method = "GET", path = "/api/projects" },
        new { method = "POST", path = "/api/projects" },
        new { method = "GET", path = "/api/projects/{id}" },
        new { method = "DELETE", path = "/api/projects/{id}" },
        new { method = "GET", path = "/api/projects/{projectId}/tasks" },
        new { method = "POST", path = "/api/projects/{projectId}/tasks" },
        new { method = "PUT", path = "/api/projects/{projectId}/tasks/{taskId}" },
        new { method = "DELETE", path = "/api/projects/{projectId}/tasks/{taskId}" },
        new { method = "POST", path = "/api/v1/projects/{projectId}/schedule" }
    }
}));

app.Run();
