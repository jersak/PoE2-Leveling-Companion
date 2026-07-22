using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoE2LevelingCompanion.Models;
using PoE2LevelingCompanion.Services;

namespace PoE2LevelingCompanion.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly LogWatcherService _logWatcher;
    private readonly CheckpointService _checkpointService;

    [ObservableProperty]
    private CharacterSession _session = new();

    [ObservableProperty]
    private string _statusText = "Waiting for PoE2...";

    public ObservableCollection<Notification> Notifications { get; } = [];

    public MainViewModel()
    {
        _logWatcher = new LogWatcherService();
        _checkpointService = new CheckpointService();
        _logWatcher.OnLogEvent += HandleLogEvent;
    }

    public void Initialize(string logFilePath, string checkpointsFilePath)
    {
        _checkpointService.Load(checkpointsFilePath);
        _logWatcher.Start(logFilePath);
        StatusText = "Watching log file...";
    }

    private void HandleLogEvent(LogEvent logEvent)
    {
        switch (logEvent.Type)
        {
            case LogEventType.SessionStart:
                StatusText = "Game started, waiting for character...";
                break;

            case LogEventType.SessionEnd:
                Session.IsActive = false;
                StatusText = "Game closed";
                break;

            case LogEventType.AreaGenerated:
                Session.CurrentAreaId = logEvent.AreaId ?? "";
                Session.CurrentAreaLevel = logEvent.AreaLevel ?? 0;

                if (logEvent.AreaId == "G1_1" && logEvent.AreaLevel == 1)
                {
                    StartNewSession(logEvent.Timestamp);
                }
                break;

            case LogEventType.ZoneEntered:
                ClearNotificationsByType(CheckpointTrigger.Zone);
                Session.CurrentZone = logEvent.ZoneName ?? "";
                EvaluateCheckpoints(logEvent);
                break;

            case LogEventType.LevelUp:
                ClearNotificationsByType(CheckpointTrigger.Level);
                if (Session.CharacterName == "Unknown" && logEvent.CharacterName != null)
                {
                    Session.CharacterName = logEvent.CharacterName;
                    Session.CharacterClass = logEvent.CharacterClass ?? "Unknown";
                }
                Session.Level = logEvent.Level ?? Session.Level;
                StatusText = $"Level {Session.Level} - {Session.CurrentZone}";
                EvaluateCheckpoints(logEvent);
                break;
        }
    }

    private void StartNewSession(DateTime timestamp)
    {
        Session = new CharacterSession
        {
            CharacterName = "Unknown",
            CharacterClass = "Unknown",
            Level = 1,
            CurrentZone = "The Riverbank",
            StartedAt = timestamp,
            IsActive = true
        };
        Notifications.Clear();
        StatusText = "New character detected!";
        OnPropertyChanged(nameof(Session));
    }

    private void EvaluateCheckpoints(LogEvent logEvent)
    {
        var triggered = _checkpointService.Evaluate(logEvent, Session);
        foreach (var cp in triggered)
        {
            Notifications.Insert(0, new Notification
            {
                Message = cp.Message,
                Timestamp = DateTime.Now,
                Category = cp.Trigger.ToString(),
                TriggerType = cp.Trigger
            });
        }

        if (triggered.Count > 0)
            SoundService.PlayPing();
    }

    private void ClearNotificationsByType(CheckpointTrigger triggerType)
    {
        for (int i = Notifications.Count - 1; i >= 0; i--)
        {
            if (Notifications[i].TriggerType == triggerType)
                Notifications.RemoveAt(i);
        }
    }

    public void ReloadCheckpoints(string checkpointsFilePath)
    {
        _checkpointService.Load(checkpointsFilePath);
    }

    [RelayCommand]
    private void DismissNotification(Notification notification)
    {
        Notifications.Remove(notification);
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        Notifications.Clear();
    }

    public void Dispose()
    {
        _logWatcher.Dispose();
    }
}
