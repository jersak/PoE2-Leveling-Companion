using System.IO;
using System.Text.Json;
using PoE2LevelingCompanion.Models;

namespace PoE2LevelingCompanion.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _filePath;

    public AppSettings Settings { get; private set; } = new();

    public SettingsService()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            Settings = new AppSettings();
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public string ResolveLogFilePath()
    {
        if (!string.IsNullOrWhiteSpace(Settings.LogFilePath) && File.Exists(Settings.LogFilePath))
            return Settings.LogFilePath;

        string[] candidates =
        [
            @"F:\Jogos\SteamLibrary\steamapps\common\Path of Exile 2\logs\client.txt",
            @"E:\Jogos\SteamLibrary\steamapps\common\Path of Exile 2\logs\client.txt",
            @"D:\SteamLibrary\steamapps\common\Path of Exile 2\logs\client.txt",
            @"C:\Program Files (x86)\Steam\steamapps\common\Path of Exile 2\logs\client.txt",
            @"C:\Program Files\Steam\steamapps\common\Path of Exile 2\logs\client.txt",
            @"C:\Program Files (x86)\Grinding Gear Games\Path of Exile 2\logs\client.txt",
        ];

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                Settings.LogFilePath = path;
                return path;
            }
        }

        return "";
    }
}
