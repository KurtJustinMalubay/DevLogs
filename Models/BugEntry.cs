namespace DevLogs.Models;

public class BugEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;   // Low | Medium | High
    public string Status { get; set; } = "Open";            // Open | In Progress | Closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Tag> Tags { get; set; } = [];
}
