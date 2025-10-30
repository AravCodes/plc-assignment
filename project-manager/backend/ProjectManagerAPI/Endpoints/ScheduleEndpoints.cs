using Microsoft.AspNetCore.Authorization;

namespace ProjectManagerAPI.Endpoints;

public static class ScheduleEndpoints
{
    public record ScheduleTaskIn(string title, int duration);
    public record ScheduleRequest(List<ScheduleTaskIn> tasks, DateOnly startDate);
    public record ScheduleTaskOut(string title, DateOnly start, DateOnly end);
    public record ScheduleResponse(List<ScheduleTaskOut> schedule);

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").RequireAuthorization();
        group.MapPost("/projects/{projectId:int}/schedule", Generate);
    }

    private static IResult Generate(int projectId, ScheduleRequest req, ProjectManagerAPI.Data.AppDbContext db, System.Security.Claims.ClaimsPrincipal user)
    {
        // Ownership check: only allow scheduling for projects owned by the current user
        var idStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (idStr is null || !int.TryParse(idStr, out var userId)) return Results.Unauthorized();
        var owns = db.Projects.Any(p => p.Id == projectId && p.UserId == userId);
        if (!owns) return Results.NotFound();

        var result = new List<ScheduleTaskOut>();
        var current = req.startDate;
        foreach (var t in req.tasks)
        {
            var start = current;
            var end = current.AddDays(Math.Max(0, t.duration - 1));
            result.Add(new ScheduleTaskOut(t.title, start, end));
            current = end.AddDays(1);
        }
        return Results.Ok(new ScheduleResponse(result));
    }
}


