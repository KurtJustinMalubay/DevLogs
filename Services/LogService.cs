using System.Collections.Concurrent;

namespace DevLogs.Services;

public record LogEntry(string Id, string Kind, string Title, string Content, string Project, DateTime CreatedAt, string[] Tags);

public record BugEntry(string Id, string Title, string Description, string Project, string Severity, string Status, DateTime CreatedAt, string[] Tags);

public record SnippetEntry(string Id, string Title, string Code, string Language, DateTime CreatedAt, string[] Tags);

public class LogService
{
    private readonly ConcurrentBag<LogEntry> _logs = new();
    private readonly ConcurrentBag<BugEntry> _bugs = new();
    private readonly ConcurrentBag<SnippetEntry> _snippets = new();

    public LogService()
    {
        // Seed logs
        _logs.Add(new(Guid.NewGuid().ToString(), "fix", "Race condition in matchmaking queue", "Fixed race condition.", "arcade-api", DateTime.Now.AddMinutes(-12), ["netcode"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "note", "Why chunk loading stutters on low-end GPUs", "Chunk loading stutters because...", "web-client", DateTime.Now.AddHours(-2), ["performance"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "snippet", "Debounced async input handler", "Here is a snippet...", "toolkit", DateTime.Now.AddDays(-1), ["async"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "idea", "Replay viewer for finished matches", "We should add a replay viewer.", "arcade-api", DateTime.Now.AddDays(-2), ["ui"]));
        _logs.Add(new(Guid.NewGuid().ToString(), "bug", "Token not refreshed after laptop sleep", "Token expires and doesn't refresh.", "web-client", DateTime.Now.AddDays(-3), ["auth"]));

        // Seed bugs
        _bugs.Add(new(Guid.NewGuid().ToString(), "Token not refreshed after laptop sleep", "Access token expires when the machine wakes from sleep and is not silently re-issued, forcing the user to log in again.", "web-client", "High", "Open", DateTime.Now.AddDays(-3), ["auth"]));
        _bugs.Add(new(Guid.NewGuid().ToString(), "Lobby list flickers on reconnect", "WebSocket reconnection causes the lobby list component to flash blank for ~300 ms.", "arcade-api", "Medium", "Open", DateTime.Now.AddDays(-6), ["netcode", "ui"]));
        _bugs.Add(new(Guid.NewGuid().ToString(), "Avatar upload silently fails on PNG > 2 MB", "The upload endpoint returns 200 but discards files over 2 MB without showing an error.", "web-client", "Low", "Closed", DateTime.Now.AddDays(-10), ["ui"]));
        _bugs.Add(new(Guid.NewGuid().ToString(), "Score not persisted on disconnect", "If a player disconnects mid-game the final score is not saved to the leaderboard.", "arcade-api", "High", "In Progress", DateTime.Now.AddDays(-1), ["netcode"]));

        // Seed snippets
        _snippets.Add(new(Guid.NewGuid().ToString(), "Debounced async input handler", "async function debounce(fn, ms = 300) {\n  let timer;\n  return (...args) => {\n    clearTimeout(timer);\n    timer = setTimeout(() => fn(...args), ms);\n  };\n}", "JavaScript", DateTime.Now.AddDays(-1), ["async", "ui"]));
        _snippets.Add(new(Guid.NewGuid().ToString(), "PBKDF2 password hash helper", "static byte[] Hash(string pw, byte[] salt) =>\n    Rfc2898DeriveBytes.Pbkdf2(pw, salt, 100_000, HashAlgorithmName.SHA256, 32);", "C#", DateTime.Now.AddDays(-4), ["auth", "security"]));
        _snippets.Add(new(Guid.NewGuid().ToString(), "CSS Grid holy grail layout", "body {\n  display: grid;\n  grid-template: auto 1fr auto / auto 1fr auto;\n}", "CSS", DateTime.Now.AddDays(-7), ["ui", "css"]));
    }

    public event Action? OnLogAdded;
    public event Action? OnBugChanged;
    public event Action? OnSnippetChanged;

    // ── Logs ────────────────────────────────────────────────────
    public void AddLog(string kind, string content)
    {
        var title = content.Split('\n').FirstOrDefault()?.Trim() ?? "Untitled";
        if (title.Length > 50) title = title[..50] + "...";

        var tags = new List<string>();
        foreach (var word in content.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
            if (word.StartsWith('#') && word.Length > 1) tags.Add(word.TrimEnd(',', '.', ';')[1..]);

        _logs.Add(new(Guid.NewGuid().ToString(), kind.ToLower(), title, content, "workspace", DateTime.Now, tags.ToArray()));
        OnLogAdded?.Invoke();
    }

    public IEnumerable<LogEntry> GetAllLogs() => _logs.OrderByDescending(l => l.CreatedAt);
    public IEnumerable<LogEntry> GetRecentLogs(int count) => GetAllLogs().Take(count);

    // ── Bugs ────────────────────────────────────────────────────
    public void AddBug(string title, string description, string project, string severity, string[] tags)
    {
        _bugs.Add(new(Guid.NewGuid().ToString(), title, description, project, severity, "Open", DateTime.Now, tags));
        OnBugChanged?.Invoke();
    }

    public void UpdateBugStatus(string id, string status)
    {
        var old = _bugs.FirstOrDefault(b => b.Id == id);
        if (old is null) return;
        // ConcurrentBag doesn't support replace in-place; rebuild around it
        var updated = old with { Status = status };
        var list = _bugs.ToList();
        list.Remove(old);
        list.Add(updated);
        while (_bugs.TryTake(out _)) { }
        foreach (var b in list) _bugs.Add(b);
        OnBugChanged?.Invoke();
    }

    public IEnumerable<BugEntry> GetAllBugs() => _bugs.OrderByDescending(b => b.CreatedAt);

    // ── Snippets ────────────────────────────────────────────────
    public void AddSnippet(string title, string code, string language, string[] tags)
    {
        _snippets.Add(new(Guid.NewGuid().ToString(), title, code, language, DateTime.Now, tags));
        OnSnippetChanged?.Invoke();
    }

    public IEnumerable<SnippetEntry> GetAllSnippets() => _snippets.OrderByDescending(s => s.CreatedAt);

    // ── Tags ────────────────────────────────────────────────────
    public IEnumerable<string> GetAllTags() =>
        _logs.SelectMany(l => l.Tags)
             .Concat(_bugs.SelectMany(b => b.Tags))
             .Concat(_snippets.SelectMany(s => s.Tags))
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .OrderBy(t => t);
}
