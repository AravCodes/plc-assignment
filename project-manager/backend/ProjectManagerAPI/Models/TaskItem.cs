using System.ComponentModel.DataAnnotations;

namespace ProjectManagerAPI.Models;

public class TaskItem
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}


