var builder = WebApplication.CreateBuilder(args);

// CORS: Allow all for local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// In-memory task repository
builder.Services.AddSingleton<TaskManagerAPI.Services.TaskRepository>();

var app = builder.Build();

app.UseCors("AllowAll");

// Minimal API endpoints for tasks scoped by project
var projects = app.MapGroup("/api/projects/{projectId:int}/tasks");

projects.MapGet("", (int projectId, TaskManagerAPI.Services.TaskRepository repo) => Results.Ok(repo.GetAll(projectId)));

projects.MapPost("", async (int projectId, TaskManagerAPI.Services.TaskRepository repo, HttpRequest request) =>
{
    var task = await request.ReadFromJsonAsync<TaskManagerAPI.Models.TaskItem>();
    if (task is null || string.IsNullOrWhiteSpace(task.Description))
    {
        return Results.BadRequest(new { message = "Description is required" });
    }
    var created = repo.Add(projectId, task.Description.Trim());
    return Results.Created($"/api/projects/{projectId}/tasks/{created.Id}", created);
});

projects.MapPut("{id:int}", async (int projectId, int id, TaskManagerAPI.Services.TaskRepository repo, HttpRequest request) =>
{
    var update = await request.ReadFromJsonAsync<TaskManagerAPI.Models.TaskItem>();
    if (update is null)
    {
        return Results.BadRequest();
    }
    var updated = repo.Update(projectId, id, update.Description, update.IsCompleted);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

projects.MapDelete("{id:int}", (int projectId, int id, TaskManagerAPI.Services.TaskRepository repo) =>
{
    var removed = repo.Delete(projectId, id);
    return removed ? Results.NoContent() : Results.NotFound();
});

// Friendly homepage with current tasks
app.MapGet("/", (HttpRequest req, TaskManagerAPI.Services.TaskRepository repo) =>
{
    int projectId = 0;
    if (req.Query.TryGetValue("projectId", out var values))
    {
        int.TryParse(values.FirstOrDefault(), out projectId);
    }
    var list = projectId > 0 ? repo.GetAll(projectId).ToArray() : Array.Empty<TaskManagerAPI.Models.TaskItem>();
    return Results.Json(new
    {
        name = "TaskManagerAPI",
        endpoints = new[]
        {
            new { method = "GET", path = "/api/projects/{projectId}/tasks" },
            new { method = "POST", path = "/api/projects/{projectId}/tasks" },
            new { method = "PUT", path = "/api/projects/{projectId}/tasks/{id}" },
            new { method = "DELETE", path = "/api/projects/{projectId}/tasks/{id}" }
        },
        count = list.Length,
        tasks = list
    });
});

app.Run();
