namespace PoE2LevelingCompanion.Models;

public sealed class Notification
{
    public required string Message { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Category { get; init; }
    public required CheckpointTrigger TriggerType { get; init; }
}
