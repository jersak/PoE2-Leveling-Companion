using System.Text.Json.Serialization;

namespace PoE2LevelingCompanion.Models;

[JsonConverter(typeof(JsonStringEnumConverter<CheckpointTrigger>))]
public enum CheckpointTrigger
{
    Level,
    Zone
}

public sealed class Checkpoint
{
    public required CheckpointTrigger Trigger { get; init; }
    public string? Class { get; init; }
    public string Message { get; init; } = "";

    // Level trigger
    public int? Level { get; init; }

    // Zone trigger
    public string? ZoneName { get; init; }

    public string Key => Trigger switch
    {
        CheckpointTrigger.Level => $"level:{Level}:{Class ?? "*"}",
        CheckpointTrigger.Zone => $"zone:{ZoneName}:{Class ?? "*"}",
        _ => $"unknown:{Message}"
    };
}

public sealed class CheckpointFile
{
    public List<Checkpoint> Checkpoints { get; init; } = [];
}
