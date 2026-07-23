using CommunityToolkit.Mvvm.ComponentModel;

namespace PoE2LevelingCompanion.Models;

public partial class CharacterSession : ObservableObject
{
    [ObservableProperty]
    private string _characterName = "Unknown";

    [ObservableProperty]
    private string _characterClass = "Unknown";

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private string _currentZone = "";

    [ObservableProperty]
    private string _currentAreaId = "";

    [ObservableProperty]
    private int _currentAreaLevel;

    [ObservableProperty]
    private DateTime _startedAt;

    [ObservableProperty]
    private bool _isActive;

    public HashSet<string> FiredCheckpointKeys { get; } = [];
}
