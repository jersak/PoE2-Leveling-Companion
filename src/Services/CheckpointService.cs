using System.IO;
using System.Text.Json;
using PoE2LevelingCompanion.Models;

namespace PoE2LevelingCompanion.Services;

public sealed class CheckpointService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<Checkpoint> _checkpoints = [];

    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _checkpoints = [];
            return;
        }

        var json = File.ReadAllText(filePath);
        var file = JsonSerializer.Deserialize<CheckpointFile>(json, JsonOptions);
        _checkpoints = file?.Checkpoints ?? [];
    }

    public List<Checkpoint> Evaluate(LogEvent logEvent, CharacterSession session)
    {
        var triggered = new List<Checkpoint>();

        foreach (var cp in _checkpoints)
        {
            if (session.FiredCheckpointKeys.Contains(cp.Key))
                continue;

            if (cp.Class != null && session.CharacterClass != "Unknown"
                && !cp.Class.Equals(session.CharacterClass, StringComparison.OrdinalIgnoreCase))
                continue;

            bool matches = cp.Trigger switch
            {
                CheckpointTrigger.Level => logEvent.Type == LogEventType.LevelUp && logEvent.Level >= cp.Level,
                CheckpointTrigger.Zone => logEvent.Type == LogEventType.ZoneEntered
                                         && cp.ZoneName != null
                                         && logEvent.ZoneName != null
                                         && logEvent.ZoneName.Equals(cp.ZoneName, StringComparison.OrdinalIgnoreCase),
                _ => false
            };

            if (matches)
            {
                session.FiredCheckpointKeys.Add(cp.Key);
                triggered.Add(cp);
            }
        }

        return triggered;
    }
}
