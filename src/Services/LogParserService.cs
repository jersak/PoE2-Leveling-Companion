using System.Text.RegularExpressions;
using PoE2LevelingCompanion.Models;

namespace PoE2LevelingCompanion.Services;

public static partial class LogParserService
{
    [GeneratedRegex(@"^(\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2})")]
    private static partial Regex TimestampRegex();

    [GeneratedRegex(@": (.+?) \((\w+)\) is now level (\d+)")]
    private static partial Regex LevelUpRegex();

    [GeneratedRegex(@"\[LOADING SCREEN\] \((.+?)\)")]
    private static partial Regex ZoneEnteredRegex();

    [GeneratedRegex(@"Generating level (\d+) area ""(.+?)""")]
    private static partial Regex AreaGeneratedRegex();

    public static LogEvent? Parse(string line)
    {
        var timestamp = ParseTimestamp(line);

        if (line.Contains("***** LOG FILE OPENING *****"))
            return new LogEvent { Type = LogEventType.SessionStart, Timestamp = timestamp };

        if (line.Contains("Closing game gracefully"))
            return new LogEvent { Type = LogEventType.SessionEnd, Timestamp = timestamp };

        var levelMatch = LevelUpRegex().Match(line);
        if (levelMatch.Success)
        {
            return new LogEvent
            {
                Type = LogEventType.LevelUp,
                Timestamp = timestamp,
                CharacterName = levelMatch.Groups[1].Value,
                CharacterClass = levelMatch.Groups[2].Value,
                Level = int.Parse(levelMatch.Groups[3].Value)
            };
        }

        var areaMatch = AreaGeneratedRegex().Match(line);
        if (areaMatch.Success)
        {
            return new LogEvent
            {
                Type = LogEventType.AreaGenerated,
                Timestamp = timestamp,
                AreaLevel = int.Parse(areaMatch.Groups[1].Value),
                AreaId = areaMatch.Groups[2].Value
            };
        }

        var zoneMatch = ZoneEnteredRegex().Match(line);
        if (zoneMatch.Success)
        {
            return new LogEvent
            {
                Type = LogEventType.ZoneEntered,
                Timestamp = timestamp,
                ZoneName = zoneMatch.Groups[1].Value
            };
        }

        return null;
    }

    private static DateTime ParseTimestamp(string line)
    {
        var match = TimestampRegex().Match(line);
        if (match.Success && DateTime.TryParseExact(match.Groups[1].Value, "yyyy/MM/dd HH:mm:ss",
                null, System.Globalization.DateTimeStyles.None, out var dt))
            return dt;
        return DateTime.Now;
    }
}
