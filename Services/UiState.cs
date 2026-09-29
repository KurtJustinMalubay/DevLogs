namespace DevLogs.Services;

/// <summary>Drives the shared "Under Development" popup.</summary>
public class UiState
{
    public string? Feature { get; private set; }
    public event Action? Changed;
    public void Show(string feature) { Feature = feature; Changed?.Invoke(); }
    public void Close() { Feature = null; Changed?.Invoke(); }
}
