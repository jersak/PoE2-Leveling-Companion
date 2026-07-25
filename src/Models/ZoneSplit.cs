namespace PoE2LevelingCompanion.Models;

public sealed class ZoneSplit
{
    public required string ZoneName { get; init; }
    public required TimeSpan Duration { get; init; }
    public TimeSpan? Delta { get; init; }
    public bool IsNewBest { get; init; }

    public string DurationText => FormatTime(Duration);

    public string DeltaText => Delta is { } d
        ? $"{(d < TimeSpan.Zero ? "-" : "+")}{FormatTime(d.Duration())}"
        : "";

    private static string FormatTime(TimeSpan t) =>
        t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss\.f") : t.ToString(@"m\:ss\.f");
}
