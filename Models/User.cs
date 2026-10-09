namespace DevLogs.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<LogEntry> LogEntries { get; set; } = [];
    public ICollection<BugEntry> BugEntries { get; set; } = [];
    public ICollection<SnippetEntry> SnippetEntries { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
}
