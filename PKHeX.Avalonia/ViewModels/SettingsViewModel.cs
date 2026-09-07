using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class SettingsViewModel : ReactiveObject
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "PKHeX", "settings.json");

    private bool _backupOnOpen = true;
    private bool _autoCheckLegality = true;
    private bool _unicodeTrainerNames = true;
    private int _defaultFormatIndex = 7; // Gen 7 by default
    private int _languageIndex;
    private string _statusMessage = string.Empty;

    public bool BackupOnOpen
    {
        get => _backupOnOpen;
        set => this.RaiseAndSetIfChanged(ref _backupOnOpen, value);
    }

    public bool AutoCheckLegality
    {
        get => _autoCheckLegality;
        set => this.RaiseAndSetIfChanged(ref _autoCheckLegality, value);
    }

    public bool UnicodeTrainerNames
    {
        get => _unicodeTrainerNames;
        set => this.RaiseAndSetIfChanged(ref _unicodeTrainerNames, value);
    }

    public int DefaultFormatIndex
    {
        get => _defaultFormatIndex;
        set => this.RaiseAndSetIfChanged(ref _defaultFormatIndex, value);
    }

    public int LanguageIndex
    {
        get => _languageIndex;
        set => this.RaiseAndSetIfChanged(ref _languageIndex, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ObservableCollection<ComboItem> Languages { get; } = [];
    public ObservableCollection<ComboItem> Formats { get; } = [];

    public SettingsViewModel()
    {
        // Languages
        Languages.Add(new ComboItem(0, "English (en)"));
        Languages.Add(new ComboItem(1, "日本語 (ja)"));
        Languages.Add(new ComboItem(2, "Français (fr)"));
        Languages.Add(new ComboItem(3, "Italiano (it)"));
        Languages.Add(new ComboItem(4, "Deutsch (de)"));
        Languages.Add(new ComboItem(5, "Español (es)"));
        Languages.Add(new ComboItem(6, "한국어 (ko)"));
        Languages.Add(new ComboItem(7, "简体中文 (zh-Hans)"));
        Languages.Add(new ComboItem(8, "繁體中文 (zh-Hant)"));

        // Generations
        Formats.Add(new ComboItem(1, "Generation 1 (R/B/Y)"));
        Formats.Add(new ComboItem(2, "Generation 2 (G/S/C)"));
        Formats.Add(new ComboItem(3, "Generation 3 (R/S/E/FR/LG)"));
        Formats.Add(new ComboItem(4, "Generation 4 (D/P/Pt/HG/SS)"));
        Formats.Add(new ComboItem(5, "Generation 5 (B/W/B2/W2)"));
        Formats.Add(new ComboItem(6, "Generation 6 (X/Y/OR/AS)"));
        Formats.Add(new ComboItem(7, "Generation 7 (S/M/US/UM/LGPE)"));
        Formats.Add(new ComboItem(8, "Generation 8 (Sw/Sh/BD/SP/LA)"));
        Formats.Add(new ComboItem(9, "Generation 9 (S/V)"));

        LoadSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("BackupOnOpen", out var b)) _backupOnOpen = b.GetBoolean();
                if (root.TryGetProperty("AutoCheckLegality", out var l)) _autoCheckLegality = l.GetBoolean();
                if (root.TryGetProperty("UnicodeTrainerNames", out var u)) _unicodeTrainerNames = u.GetBoolean();
                if (root.TryGetProperty("DefaultFormatIndex", out var df)) _defaultFormatIndex = df.GetInt32();
                if (root.TryGetProperty("LanguageIndex", out var li)) _languageIndex = li.GetInt32();
            }
        }
        catch
        {
            // fallback to defaults
        }
    }

    public void SaveSettings()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);

            using var stream = File.Create(SettingsPath);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

            writer.WriteStartObject();
            writer.WriteBoolean("BackupOnOpen", _backupOnOpen);
            writer.WriteBoolean("AutoCheckLegality", _autoCheckLegality);
            writer.WriteBoolean("UnicodeTrainerNames", _unicodeTrainerNames);
            writer.WriteNumber("DefaultFormatIndex", _defaultFormatIndex);
            writer.WriteNumber("LanguageIndex", _languageIndex);
            writer.WriteEndObject();

            StatusMessage = "Settings successfully saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
    }
}
