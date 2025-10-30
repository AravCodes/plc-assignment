using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagerAPI.Data;
using ProjectManagerAPI.Dtos;
using ProjectManagerAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectManagerAPI.Endpoints;

public static class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapGet("/me", [Authorize] (ClaimsPrincipal user) => Results.Ok(new { email = user.Identity?.Name }));
    }

    private static async Task<IResult> Register(RegisterRequest request, AppDbContext db, IPasswordHasher<User> hasher)
    {
        if (!MiniValidator.MiniValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Results.BadRequest(new { message = "Email already registered" });
        }

        var user = new User { Email = request.Email };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Results.Created($"/api/auth/me", new { user.Id, user.Email });
    }

    private static async Task<IResult> Login(LoginRequest request, AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config)
    {
        if (!MiniValidator.MiniValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
        {
            return Results.BadRequest(new { message = "Invalid credentials" });
        }
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Results.BadRequest(new { message = "Invalid credentials" });
        }

        var token = GenerateJwt(user, config);
        return Results.Ok(new AuthResponse { Token = token });
    }

    private static string GenerateJwt(User user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"] ?? "dev-secret-change-me"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var issuer = config["Jwt:Issuer"] ?? "pm.local";
        var audience = config["Jwt:Audience"] ?? "pm.local";
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


