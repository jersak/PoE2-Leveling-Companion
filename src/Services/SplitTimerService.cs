using System.IO;
using System.Text.Json;

namespace PoE2LevelingCompanion.Services;

public sealed class SplitTimerService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private Dictionary<string, long> _bestTimes = new();
    private string _filePath = "";

    public void Load(string filePath)
    {
        _filePath = filePath;
        if (!File.Exists(filePath))
        {
            _bestTimes = new();
            return;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            _bestTimes = JsonSerializer.Deserialize<Dictionary<string, long>>(json, JsonOptions) ?? new();
        }
        catch
        {
            _bestTimes = new();
        }
    }

    public TimeSpan? GetBestTime(string zoneName)
    {
        return _bestTimes.TryGetValue(zoneName, out var ticks) ? TimeSpan.FromTicks(ticks) : null;
    }

    public bool UpdateBestTime(string zoneName, TimeSpan duration)
    {
        if (!_bestTimes.TryGetValue(zoneName, out var existingTicks) || duration.Ticks < existingTicks)
        {
            _bestTimes[zoneName] = duration.Ticks;
            Save();
            return true;
        }
        return false;
    }

    public void Reset()
    {
        _bestTimes.Clear();
        Save();
    }

    private void Save()
    {
        if (string.IsNullOrEmpty(_filePath)) return;

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_bestTimes, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
