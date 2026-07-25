using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoE2LevelingCompanion.Models;

namespace PoE2LevelingCompanion.ViewModels;

public sealed class CheckpointEntryViewModel
{
    public CheckpointTrigger Trigger { get; init; }
    public string? ZoneName { get; init; }
    public int? Level { get; init; }
    public required string Message { get; init; }
    public string? ClassFilter { get; init; }

    public string TriggerLabel => Trigger == CheckpointTrigger.Zone
        ? ZoneName ?? ""
        : $"Level {Level}";

    public string TriggerBadge => Trigger == CheckpointTrigger.Zone ? "Zone" : "Level";
    public string ClassDisplay => ClassFilter ?? "Any";
}

public partial class CheckpointEditorViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string _filePath = "";

    public ObservableCollection<CheckpointEntryViewModel> Entries { get; } = [];

    public static string[] ClassOptionsStatic { get; } = ["Any", .. ZoneData.Classes];
    public static CheckpointTrigger[] TriggerOptions { get; } = [CheckpointTrigger.Zone, CheckpointTrigger.Level];

    [ObservableProperty]
    private CheckpointTrigger _newTrigger = CheckpointTrigger.Zone;

    [ObservableProperty]
    private string _newValue = "";

    [ObservableProperty]
    private string _newMessage = "";

    [ObservableProperty]
    private string _newClass = "Any";

    [ObservableProperty]
    private string _statusMessage = "";

    public bool IsZoneTrigger => NewTrigger == CheckpointTrigger.Zone;

    partial void OnNewTriggerChanged(CheckpointTrigger value)
    {
        OnPropertyChanged(nameof(IsZoneTrigger));
        NewValue = "";
    }

    public void Load(string filePath)
    {
        _filePath = filePath;
        if (!File.Exists(filePath)) return;

        try
        {
            var json = File.ReadAllText(filePath);
            var file = JsonSerializer.Deserialize<CheckpointFile>(json, JsonOptions);
            if (file == null) return;

            foreach (var cp in file.Checkpoints)
            {
                Entries.Add(new CheckpointEntryViewModel
                {
                    Trigger = cp.Trigger,
                    ZoneName = cp.ZoneName,
                    Level = cp.Level,
                    Message = cp.Message,
                    ClassFilter = cp.Class
                });
            }
        }
        catch { }
    }

    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
        {
            StatusMessage = "Message is required";
            return;
        }

        if (NewTrigger == CheckpointTrigger.Zone)
        {
            if (string.IsNullOrWhiteSpace(NewValue))
            {
                StatusMessage = "Zone name is required";
                return;
            }

            Entries.Add(new CheckpointEntryViewModel
            {
                Trigger = CheckpointTrigger.Zone,
                ZoneName = NewValue.Trim(),
                Message = NewMessage.Trim(),
                ClassFilter = NewClass is "Any" ? null : NewClass
            });
        }
        else
        {
            if (!int.TryParse(NewValue, out var level) || level < 1 || level > 100)
            {
                StatusMessage = "Level must be between 1 and 100";
                return;
            }

            Entries.Add(new CheckpointEntryViewModel
            {
                Trigger = CheckpointTrigger.Level,
                Level = level,
                Message = NewMessage.Trim(),
                ClassFilter = NewClass is "Any" ? null : NewClass
            });
        }

        NewMessage = "";
        StatusMessage = "";
    }

    [RelayCommand]
    private void Remove(CheckpointEntryViewModel entry)
    {
        Entries.Remove(entry);
    }

    [RelayCommand]
    private void Save()
    {
        var checkpoints = Entries.Select(e => new Checkpoint
        {
            Trigger = e.Trigger,
            ZoneName = e.ZoneName,
            Level = e.Level,
            Message = e.Message,
            Class = e.ClassFilter
        }).ToList();

        var file = new CheckpointFile { Checkpoints = checkpoints };
        var json = JsonSerializer.Serialize(file, JsonOptions);

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_filePath, json);
        StatusMessage = $"Saved {checkpoints.Count} checkpoints";
    }
}
