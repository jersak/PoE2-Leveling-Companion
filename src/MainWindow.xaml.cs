using System.Windows;
using System.Windows.Input;
using PoE2LevelingCompanion.Services;
using PoE2LevelingCompanion.ViewModels;

namespace PoE2LevelingCompanion;

public partial class MainWindow : Window
{
    private readonly SettingsService _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        _settings.Load();
        RestoreWindowPosition();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            var logPath = _settings.ResolveLogFilePath();
            var checkpointsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "checkpoints.json");

            if (string.IsNullOrEmpty(logPath))
                vm.StatusText = "Log file not found — set path in appsettings.json";
            else
                vm.Initialize(logPath, checkpointsPath);
        }
    }

    private void RestoreWindowPosition()
    {
        var s = _settings.Settings;
        Width = s.WindowWidth;
        Height = s.WindowHeight;

        if (!double.IsNaN(s.WindowLeft) && !double.IsNaN(s.WindowTop))
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = s.WindowLeft;
            Top = s.WindowTop;
        }
    }

    private void SaveWindowPosition()
    {
        var s = _settings.Settings;
        s.WindowLeft = Left;
        s.WindowTop = Top;
        s.WindowWidth = Width;
        s.WindowHeight = Height;
        _settings.Save();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }

    private void EditCheckpoints_Click(object sender, RoutedEventArgs e)
    {
        var checkpointsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "checkpoints.json");
        var editor = new CheckpointEditorWindow(checkpointsPath) { Owner = this, Topmost = true };
        editor.ShowDialog();

        if (DataContext is MainViewModel vm)
            vm.ReloadCheckpoints(checkpointsPath);
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        SaveWindowPosition();
        if (DataContext is MainViewModel vm)
            vm.Dispose();
        Close();
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
    }
}
