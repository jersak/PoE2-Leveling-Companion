using System.Windows;
using Microsoft.Win32;
using PoE2LevelingCompanion.Services;
using PoE2LevelingCompanion.ViewModels;

namespace PoE2LevelingCompanion;

public partial class CheckpointEditorWindow : Window
{
    private readonly SettingsService? _settings;

    public event Action<string>? LogFilePathChanged;

    public CheckpointEditorWindow(string checkpointsFilePath, SettingsService? settings = null)
    {
        _settings = settings;
        InitializeComponent();

        if (settings != null && !string.IsNullOrEmpty(settings.Settings.LogFilePath))
            LogFilePathBox.Text = settings.Settings.LogFilePath;

        if (DataContext is CheckpointEditorViewModel vm)
            vm.Load(checkpointsFilePath);
    }

    private void BrowseLogFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select PoE2 client.txt Log File",
            Filter = "Log files (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = "client.txt"
        };

        var currentPath = LogFilePathBox.Text;
        if (!string.IsNullOrEmpty(currentPath))
        {
            var dir = System.IO.Path.GetDirectoryName(currentPath);
            if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
                dialog.InitialDirectory = dir;
        }

        if (dialog.ShowDialog(this) == true)
        {
            LogFilePathBox.Text = dialog.FileName;
            if (_settings != null)
            {
                _settings.Settings.LogFilePath = dialog.FileName;
                _settings.Save();
            }
            LogFilePathChanged?.Invoke(dialog.FileName);
        }
    }
}
