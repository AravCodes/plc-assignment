using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProjectManagerAPI.Data;
using ProjectManagerAPI.Dtos;
using ProjectManagerAPI.Models;
using System.Security.Claims;

namespace ProjectManagerAPI.Endpoints;

public static class ProjectEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        // Projects
        group.MapGet("/projects", GetProjects);
        group.MapPost("/projects", CreateProject);
        group.MapGet("/projects/{id:int}", GetProject);
        group.MapGet("/projects/{id:int}/tasks-ui", RedirectToExternalTaskManager);
        group.MapDelete("/projects/{id:int}", DeleteProject);

        // Tasks
        group.MapGet("/projects/{projectId:int}/tasks", GetTasks);
        group.MapPost("/projects/{projectId:int}/tasks", CreateTask);
        group.MapPut("/projects/{projectId:int}/tasks/{taskId:int}", UpdateTask);
        group.MapDelete("/projects/{projectId:int}/tasks/{taskId:int}", DeleteTask);
    }

    private static async Task<IResult> RedirectToExternalTaskManager(HttpContext http, int id, AppDbContext db, IConfiguration config)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();

        // Ensure the project exists and belongs to the current user
        var exists = await db.Projects.AsNoTracking().AnyAsync(p => p.Id == id && p.UserId == userId);
        if (!exists) return Results.NotFound();

        var baseUrl = config["TaskManager:BaseUrl"] ?? "http://localhost:3001";
        var url = string.Concat(baseUrl.TrimEnd('/'), "/?projectId=", id);
        return Results.Redirect(url, permanent: false);
    }

    private static async Task<IResult> GetProjects(HttpContext http, AppDbContext db, int page = 1, int pageSize = 20)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = db.Projects.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt).AsNoTracking();
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var result = new
        {
            page,
            pageSize,
            total,
            data = items.Select(p => new ProjectDto { Id = p.Id, Title = p.Title, Description = p.Description, CreatedAt = p.CreatedAt })
        };
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateProject(HttpContext http, CreateProjectRequest request, AppDbContext db)
    {
        if (!MiniValidator.MiniValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        var project = new Project { Title = request.Title, Description = request.Description, UserId = userId.Value };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return Results.Created($"/api/projects/{project.Id}", new ProjectDto { Id = project.Id, Title = project.Title, Description = project.Description, CreatedAt = project.CreatedAt });
    }

    private static async Task<IResult> GetProject(HttpContext http, int id, AppDbContext db)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        var project = await db.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project is null) return Results.NotFound();
    var dto = new ProjectDto { Id = project.Id, Title = project.Title, Description = project.Description, CreatedAt = project.CreatedAt };
    var tasks = project.Tasks.Select(t => new TaskDto { Id = t.Id, Title = t.Title, IsCompleted = t.IsCompleted, DueDate = t.DueDate });
        return Results.Ok(new { project = dto, tasks });
    }

    private static async Task<IResult> DeleteProject(HttpContext http, int id, AppDbContext db)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project is null) return Results.NotFound();
        db.Projects.Remove(project);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetTasks(HttpContext http, int projectId, AppDbContext db)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        var isOwner = await db.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
        if (!isOwner) return Results.NotFound();
    var tasks = await db.Tasks.Where(t => t.ProjectId == projectId).AsNoTracking().ToListAsync();
    return Results.Ok(tasks.Select(t => new TaskDto { Id = t.Id, Title = t.Title, IsCompleted = t.IsCompleted, DueDate = t.DueDate }));
    }

    private static async Task<IResult> CreateTask(HttpContext http, int projectId, CreateTaskRequest request, AppDbContext db)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        if (!await db.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId)) return Results.NotFound();
        if (!MiniValidator.MiniValidator.TryValidate(request, out var errors)) return Results.ValidationProblem(errors);
    var task = new TaskItem { ProjectId = projectId, Title = request.Title, IsCompleted = false, DueDate = request.DueDate };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    return Results.Created($"/api/projects/{projectId}/tasks/{task.Id}", new TaskDto { Id = task.Id, Title = task.Title, IsCompleted = task.IsCompleted, DueDate = task.DueDate });
    }

    private static async Task<IResult> UpdateTask(HttpContext http, int projectId, int taskId, TaskDto request, AppDbContext db)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        var task = await db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId && t.Project!.UserId == userId);
        if (task is null) return Results.NotFound();
    task.Title = request.Title;
    task.IsCompleted = request.IsCompleted;
    task.DueDate = request.DueDate;
        await db.SaveChangesAsync();
    return Results.Ok(new TaskDto { Id = task.Id, Title = task.Title, IsCompleted = task.IsCompleted, DueDate = task.DueDate });
    }

    private static async Task<IResult> DeleteTask(HttpContext http, int projectId, int taskId, AppDbContext db)
    {
        var userId = GetUserId(http.User);
        if (userId is null) return Results.Unauthorized();
        var task = await db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId && t.Project!.UserId == userId);
        if (task is null) return Results.NotFound();
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static int? GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(id, out var intId)) return intId;
        return null;
    }
}


