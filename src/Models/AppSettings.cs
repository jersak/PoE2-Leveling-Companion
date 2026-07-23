namespace PoE2LevelingCompanion.Models;

public sealed class AppSettings
{
    public string LogFilePath { get; set; } = "";
    public double WindowLeft { get; set; } = double.NaN;
    public double WindowTop { get; set; } = double.NaN;
    public double WindowWidth { get; set; } = 360;
    public double WindowHeight { get; set; } = 300;
}
