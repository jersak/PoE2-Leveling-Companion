namespace PoE2LevelingCompanion.Models;

public enum LogEventType
{
    LevelUp,
    ZoneEntered,
    AreaGenerated,
    SessionStart,
    SessionEnd
}

public sealed class LogEvent
{
    public required LogEventType Type { get; init; }
    public required DateTime Timestamp { get; init; }

    // LevelUp
    public string? CharacterName { get; init; }
    public string? CharacterClass { get; init; }
    public int? Level { get; init; }

    // ZoneEntered (from LOADING SCREEN)
    public string? ZoneName { get; init; }

    // AreaGenerated
    public string? AreaId { get; init; }
    public int? AreaLevel { get; init; }
}
