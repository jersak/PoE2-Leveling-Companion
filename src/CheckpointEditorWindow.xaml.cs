using System.Windows;
using PoE2LevelingCompanion.ViewModels;

namespace PoE2LevelingCompanion;

public partial class CheckpointEditorWindow : Window
{
    public CheckpointEditorWindow(string checkpointsFilePath)
    {
        InitializeComponent();

        if (DataContext is CheckpointEditorViewModel vm)
            vm.Load(checkpointsFilePath);
    }
}
