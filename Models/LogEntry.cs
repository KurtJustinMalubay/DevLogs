namespace DevLogs.Models;

public class LogEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Kind { get; set; } = string.Empty;       // log | fix | note | snippet | idea | bug
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Tag> Tags { get; set; } = [];
}
