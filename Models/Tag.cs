namespace DevLogs.Models;

public class Tag
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<LogEntry> LogEntries { get; set; } = [];
    public ICollection<BugEntry> BugEntries { get; set; } = [];
    public ICollection<SnippetEntry> SnippetEntries { get; set; } = [];
}
