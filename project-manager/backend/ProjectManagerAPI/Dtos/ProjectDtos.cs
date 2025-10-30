using System.ComponentModel.DataAnnotations;

namespace ProjectManagerAPI.Dtos;

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
}

public class CreateTaskRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

public class PageRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}


