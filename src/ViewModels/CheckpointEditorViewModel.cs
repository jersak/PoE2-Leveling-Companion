using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoE2LevelingCompanion.Models;

namespace PoE2LevelingCompanion.ViewModels;

public partial class ZoneEntryViewModel : ObservableObject
{
    public string ZoneName { get; init; } = "";
    public string Act { get; init; } = "";

    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private string? _classFilter;

    [ObservableProperty]
    private bool _hasCheckpoint;
}

public partial class LevelEntryViewModel : ObservableObject
{
    public int Level { get; init; }

    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private string? _classFilter;

    [ObservableProperty]
    private bool _hasCheckpoint;
}

public partial class CheckpointEditorViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string _filePath = "";

    public ObservableCollection<ZoneEntryViewModel> ZoneEntries { get; } = [];
    public ObservableCollection<LevelEntryViewModel> LevelEntries { get; } = [];
    public ObservableCollection<ZoneEntryViewModel> FilteredZoneEntries { get; } = [];
    public static string[] ClassOptionsStatic { get; } = ["Any", .. ZoneData.Classes];
    public string[] ClassOptions => ClassOptionsStatic;

    [ObservableProperty]
    private string _zoneSearchText = "";

    [ObservableProperty]
    private string _statusMessage = "";

    partial void OnZoneSearchTextChanged(string value)
    {
        ApplyZoneFilter();
    }

    public void Load(string checkpointsFilePath)
    {
        _filePath = checkpointsFilePath;

        foreach (var (act, zones) in ZoneData.CampaignZones)
        {
            foreach (var zone in zones)
            {
                ZoneEntries.Add(new ZoneEntryViewModel { ZoneName = zone, Act = act });
            }
        }

        for (int i = 1; i <= 100; i++)
        {
            LevelEntries.Add(new LevelEntryViewModel { Level = i });
        }

        if (File.Exists(checkpointsFilePath))
        {
            try
            {
                var json = File.ReadAllText(checkpointsFilePath);
                var file = JsonSerializer.Deserialize<CheckpointFile>(json, JsonOptions);
                if (file != null)
                    ApplyExistingCheckpoints(file.Checkpoints);
            }
            catch { }
        }

        ApplyZoneFilter();
    }

    private void ApplyExistingCheckpoints(List<Checkpoint> checkpoints)
    {
        foreach (var cp in checkpoints)
        {
            if (cp.Trigger == CheckpointTrigger.Zone && cp.ZoneName != null)
            {
                var entry = ZoneEntries.FirstOrDefault(z =>
                    z.ZoneName.Equals(cp.ZoneName, StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    entry.HasCheckpoint = true;
                    entry.Message = cp.Message;
                    entry.ClassFilter = cp.Class;
                }
            }
            else if (cp.Trigger == CheckpointTrigger.Level && cp.Level is > 0 and <= 100)
            {
                var entry = LevelEntries.FirstOrDefault(l => l.Level == cp.Level);
                if (entry != null)
                {
                    entry.HasCheckpoint = true;
                    entry.Message = cp.Message;
                    entry.ClassFilter = cp.Class;
                }
            }
        }
    }

    private void ApplyZoneFilter()
    {
        FilteredZoneEntries.Clear();
        var search = ZoneSearchText.Trim();

        foreach (var entry in ZoneEntries)
        {
            if (string.IsNullOrEmpty(search)
                || entry.ZoneName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || entry.Act.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredZoneEntries.Add(entry);
            }
        }
    }

    [RelayCommand]
    private void Save()
    {
        var checkpoints = new List<Checkpoint>();

        foreach (var zone in ZoneEntries.Where(z => z.HasCheckpoint && !string.IsNullOrWhiteSpace(z.Message)))
        {
            checkpoints.Add(new Checkpoint
            {
                Trigger = CheckpointTrigger.Zone,
                ZoneName = zone.ZoneName,
                Message = zone.Message.Trim(),
                Class = zone.ClassFilter is "Any" or null ? null : zone.ClassFilter
            });
        }

        foreach (var level in LevelEntries.Where(l => l.HasCheckpoint && !string.IsNullOrWhiteSpace(l.Message)))
        {
            checkpoints.Add(new Checkpoint
            {
                Trigger = CheckpointTrigger.Level,
                Level = level.Level,
                Message = level.Message.Trim(),
                Class = level.ClassFilter is "Any" or null ? null : level.ClassFilter
            });
        }

        var file = new CheckpointFile { Checkpoints = checkpoints };
        var json = JsonSerializer.Serialize(file, JsonOptions);
        File.WriteAllText(_filePath, json);

        StatusMessage = $"Saved {checkpoints.Count} checkpoints";
    }
}
