using System.Text.Json;

namespace MagicMove.Services;

public sealed class JsonSettingsRepository : ISettingsRepository
{
    private static readonly string _settingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MagicMove"
    );

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    private static readonly string _settingsPath = Path.Combine(_settingsDir, "settings.json");

    private readonly List<MoveEntry> _entries;

    public JsonSettingsRepository()
    {
        _entries = LoadFromDisk();
    }

    public IReadOnlyList<MoveEntry> GetEntries() => _entries.AsReadOnly();

    public void AddEntry(MoveEntry entry)
    {
        _entries.Add(entry);
        SaveToDisk();
    }

    public void RemoveEntry(MoveEntry entry)
    {
        _entries.Remove(entry);
        SaveToDisk();
    }

    private static List<MoveEntry> LoadFromDisk()
    {
        if (!File.Exists(_settingsPath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            return data?.Entries ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void SaveToDisk()
    {
        Directory.CreateDirectory(_settingsDir);
        var data = new SettingsData { Entries = _entries };
        var json = JsonSerializer.Serialize(
            data,
            _jsonOptions
        );
        File.WriteAllText(_settingsPath, json);
    }

    private sealed class SettingsData
    {
        public List<MoveEntry> Entries { get; set; } = [];
    }
}
