using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoE2LevelingCompanion.Models;
using PoE2LevelingCompanion.Services;

namespace PoE2LevelingCompanion.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly LogWatcherService _logWatcher;
    private readonly CheckpointService _checkpointService;
    private readonly SplitTimerService _splitTimer;
    private readonly DispatcherTimer _elapsedTimer;

    private DateTime? _zoneEnteredAt;
    private string? _previousZoneName;
    private readonly HashSet<string> _visitedZones = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private CharacterSession _session = new();

    [ObservableProperty]
    private string _statusText = "Waiting for PoE2...";

    [ObservableProperty]
    private string _currentZoneElapsed = "";

    [ObservableProperty]
    private string _totalRunElapsed = "";

    public ObservableCollection<Notification> Notifications { get; } = [];
    public ObservableCollection<ZoneSplit> Splits { get; } = [];

    public MainViewModel()
    {
        _logWatcher = new LogWatcherService();
        _checkpointService = new CheckpointService();
        _splitTimer = new SplitTimerService();
        _logWatcher.OnLogEvent += HandleLogEvent;

        _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _elapsedTimer.Tick += (_, _) => UpdateElapsedTime();
    }

    public void Initialize(string logFilePath, string checkpointsFilePath, string bestTimesFilePath)
    {
        _checkpointService.Load(checkpointsFilePath);
        _splitTimer.Load(bestTimesFilePath);
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
                RecordCurrentZoneSplit(logEvent.Timestamp);
                Session.IsActive = false;
                StatusText = "Game closed";
                _elapsedTimer.Stop();
                CurrentZoneElapsed = "";
                break;


            case LogEventType.AreaGenerated:
                Session.CurrentAreaId = logEvent.AreaId ?? "";
                Session.CurrentAreaLevel = logEvent.AreaLevel ?? 0;

                if (logEvent.AreaId == "G1_1" && logEvent.AreaLevel == 1)
                {
                    RecordCurrentZoneSplit(logEvent.Timestamp);
                    StartNewSession(logEvent.Timestamp);
                }
                break;

            case LogEventType.ZoneEntered:
                var isNewZone = logEvent.ZoneName != null && logEvent.ZoneName != Session.CurrentZone;
                Session.CurrentZone = logEvent.ZoneName ?? "";

                if (isNewZone)
                {
                    ClearNotificationsByType(CheckpointTrigger.Zone);
                    EvaluateCheckpoints(logEvent);

                    if (_visitedZones.Add(logEvent.ZoneName!))
                    {
                        RecordCurrentZoneSplit(logEvent.Timestamp);
                        _zoneEnteredAt = logEvent.Timestamp;
                        _previousZoneName = logEvent.ZoneName;
                        _elapsedTimer.Start();
                    }
                }
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

    private void RecordCurrentZoneSplit(DateTime exitTimestamp)
    {
        if (!_zoneEnteredAt.HasValue || string.IsNullOrEmpty(_previousZoneName))
            return;

        var duration = exitTimestamp - _zoneEnteredAt.Value;
        if (duration <= TimeSpan.Zero)
            return;

        var bestBefore = _splitTimer.GetBestTime(_previousZoneName);
        var isNewBest = _splitTimer.UpdateBestTime(_previousZoneName, duration);
        TimeSpan? delta = bestBefore.HasValue ? duration - bestBefore.Value : null;

        Splits.Add(new ZoneSplit
        {
            ZoneName = _previousZoneName,
            Duration = duration,
            Delta = delta,
            IsNewBest = isNewBest
        });

        _zoneEnteredAt = null;
        _previousZoneName = null;
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
        Splits.Clear();
        _visitedZones.Clear();
        _zoneEnteredAt = null;
        _previousZoneName = null;
        CurrentZoneElapsed = "";
        TotalRunElapsed = "";
        _elapsedTimer.Start();
        StatusText = "New character detected!";
        OnPropertyChanged(nameof(Session));
    }

    private void UpdateElapsedTime()
    {
        if (_zoneEnteredAt.HasValue)
        {
            var elapsed = DateTime.Now - _zoneEnteredAt.Value;
            CurrentZoneElapsed = FormatElapsed(elapsed);
        }

        if (Session.IsActive && Session.StartedAt != default)
        {
            var total = DateTime.Now - Session.StartedAt;
            TotalRunElapsed = FormatElapsed(total);
        }
    }

    private static string FormatElapsed(TimeSpan t) =>
        t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss\.f") : t.ToString(@"m\:ss\.f");

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

    [RelayCommand]
    private void ResetBestTimes()
    {
        _splitTimer.Reset();
    }

    public void Dispose()
    {
        _elapsedTimer.Stop();
        _logWatcher.Dispose();
    }
}
