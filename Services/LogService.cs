using System.Collections.Concurrent;

namespace DevLogs.Services;

public record LogEntry(string Id, string Kind, string Title, string Content, string Project, DateTime CreatedAt, string[] Tags);

public class LogService
{
    private readonly ConcurrentBag<LogEntry> _logs = new();

    public LogService()
    {
        // Seed data
        _logs.Add(new(Guid.NewGuid().ToString(), "fix", "Race condition in matchmaking queue", "Fixed race condition.", "arcade-api", DateTime.Now.AddMinutes(-12), ["netcode"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "note", "Why chunk loading stutters on low-end GPUs", "Chunk loading stutters because...", "web-client", DateTime.Now.AddHours(-2), ["performance"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "snippet", "Debounced async input handler", "Here is a snippet...", "toolkit", DateTime.Now.AddDays(-1), ["async"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "idea", "Replay viewer for finished matches", "We should add a replay viewer.", "arcade-api", DateTime.Now.AddDays(-2), ["ui"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "bug", "Token not refreshed after laptop sleep", "Token expires and doesn't refresh.", "web-client", DateTime.Now.AddDays(-3), ["auth"]));
    }

    public event Action? OnLogAdded;

    public void AddLog(string kind, string content)
    {
        var title = content.Split('\n').FirstOrDefault()?.Trim() ?? "Untitled";
        if (title.Length > 50) title = title[..50] + "...";
        
        var tags = new List<string>();
        foreach (var word in content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.StartsWith("#") && word.Length > 1) tags.Add(word.TrimEnd(',', '.', ';')[1..]);
        }

        var entry = new LogEntry(
            Guid.NewGuid().ToString(),
            kind.ToLower(),
            title,
            content,
            "workspace",
            DateTime.Now,
            tags.ToArray()
        );
        _logs.Add(entry);
        OnLogAdded?.Invoke();
    }

    public IEnumerable<LogEntry> GetAllLogs() => _logs.OrderByDescending(l => l.CreatedAt);
    public IEnumerable<LogEntry> GetRecentLogs(int count) => GetAllLogs().Take(count);
}
