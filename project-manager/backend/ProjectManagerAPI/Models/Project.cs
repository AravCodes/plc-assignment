using System.ComponentModel.DataAnnotations;

namespace ProjectManagerAPI.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Ownership: each project belongs to a single user
    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}


