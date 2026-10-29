using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using PKHeX.Core;
using PKHeX.Drawing;
using PKHeX.Drawing.Misc;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class MainViewModel : ReactiveObject
{
    private SaveFile? _saveFile;
    private string _windowTitle = "PKHeX (macOS)";
    private string _saveFilePath = string.Empty;
    private string _gameVersionName = "No Save Loaded";
    private string _trainerName = "-";
    private string _trainerId = "-";
    private string _secretId = "-";
    private uint _money;
    private string _playTime = "0:00";
    private int _currentBoxIndex;
    private string _statusMessage = "Ready. Open a save file or drag and drop onto window.";
    private RawImage? _boxWallpaper;

    public PKMEditorViewModel Editor { get; } = new();
    public BoxViewerViewModel BoxViewer { get; } = new();

    public ObservableCollection<string> BoxNames { get; } = [];

    public SaveFile? SaveFile
    {
        get => _saveFile;
        private set
        {
            this.RaiseAndSetIfChanged(ref _saveFile, value);
            this.RaisePropertyChanged(nameof(HasSaveFile));
        }
    }

    public bool HasSaveFile => _saveFile != null;

    public string WindowTitle
    {
        get => _windowTitle;
        set => this.RaiseAndSetIfChanged(ref _windowTitle, value);
    }

    public string SaveFilePath
    {
        get => _saveFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _saveFilePath, value);
            this.RaisePropertyChanged(nameof(SaveFileName));
        }
    }

    public string SaveFileName => !string.IsNullOrEmpty(_saveFilePath) ? Path.GetFileName(_saveFilePath) : "No Save Loaded";

    public string GameVersionName
    {
        get => _gameVersionName;
        set => this.RaiseAndSetIfChanged(ref _gameVersionName, value);
    }

    public string TrainerName
    {
        get => _trainerName;
        set
        {
            this.RaiseAndSetIfChanged(ref _trainerName, value);
            if (_saveFile != null && !string.IsNullOrEmpty(value))
                _saveFile.OT = value;
        }
    }

    public string TrainerID
    {
        get => _trainerId;
        set => this.RaiseAndSetIfChanged(ref _trainerId, value);
    }

    public string SecretID
    {
        get => _secretId;
        set => this.RaiseAndSetIfChanged(ref _secretId, value);
    }

    public uint Money
    {
        get => _money;
        set
        {
            this.RaiseAndSetIfChanged(ref _money, value);
            this.RaisePropertyChanged(nameof(FormattedMoney));
            if (_saveFile != null)
                _saveFile.Money = value;
        }
    }

    public string FormattedMoney => $"${_money:N0}";

    public string PlayTime
    {
        get => _playTime;
        set => this.RaiseAndSetIfChanged(ref _playTime, value);
    }

    public int CurrentBoxIndex
    {
        get => _currentBoxIndex;
        set
        {
            if (_saveFile == null)
                return;
            int clamped = Math.Clamp(value, 0, Math.Max(0, _saveFile.BoxCount - 1));
            this.RaiseAndSetIfChanged(ref _currentBoxIndex, clamped);
            BoxViewer.LoadBox(_saveFile, clamped);
            UpdateBoxDetails();
        }
    }

    public string BoxOccupancy
    {
        get
        {
            if (_saveFile == null) return "0 / 30 PKM";
            var boxData = _saveFile.GetBoxData(_currentBoxIndex);
            int count = boxData.Count(p => p.Species != 0);
            return $"{count} / 30 PKM";
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    // Contextual Editor Visibility (mirroring PKHeX WinForms SAVEditor.cs:1300-1345)
    public bool HasInventory => _saveFile != null && ((_saveFile.HasParty && _saveFile is not SAV4BR) || _saveFile is SAV7b);
    public bool HasTrainerInfo => _saveFile != null && (_saveFile.HasParty || _saveFile is SAV7b);
    public bool HasParty => _saveFile != null && _saveFile.HasParty;
    public bool HasDaycare => _saveFile is IDaycareStorage or IDaycareMulti;
    public bool HasPokedex => _saveFile != null && _saveFile.HasPokeDex;
    public bool HasEventFlags => _saveFile is IEventFlag37 or IEventFlagProvider37 or SAV1 or SAV2 or SAV8BS or SAV7b or SAV9ZA;
    public bool HasMysteryGift => _saveFile is IMysteryGiftStorageProvider;
    public bool HasHallOfFame => _saveFile is ISaveBlock6Main or SAV7 or SAV3 { IsMisconfiguredSize: false } or SAV1;
    public bool HasDonuts => _saveFile is SAV9ZA { SaveRevision: >= 1 };
    public bool HasFashion => _saveFile is SAV9SV or SAV9ZA;
    public bool HasBlockInspector => _saveFile is ISCBlockArray;
    public bool IsZA => _saveFile is SAV9ZA;

    public RawImage? BoxWallpaper
    {
        get => _boxWallpaper;
        private set => this.RaiseAndSetIfChanged(ref _boxWallpaper, value);
    }

    public MainViewModel()
    {
        BoxViewer.OnSlotSelected = pk => Editor.PopulateFields(pk);
    }

    public bool LoadFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                StatusMessage = $"File not found: {path}";
                return false;
            }

            var obj = FileUtil.GetSupportedFile(path, _saveFile);
            if (obj is SaveFile sav)
            {
                return LoadSave(sav, path);
            }
            if (obj is PKM pkm)
            {
                Editor.PopulateFields(pkm);
                StatusMessage = $"Loaded Pokémon: {pkm.Nickname} ({(Species)pkm.Species})";
                return true;
            }

            StatusMessage = $"Unsupported file format: {Path.GetFileName(path)}";
            return false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
            return false;
        }
    }

    public bool LoadSave(SaveFile sav, string path)
    {
        sav.Metadata.SetExtraInfo(path);
        SpriteUtil.Initialize(sav);
        GameInfo.FilteredSources = new FilteredGameDataSource(sav, GameInfo.Sources, false);

        SaveFile = sav;
        SaveFilePath = path;
        WindowTitle = $"PKHeX - {sav.Version} - {Path.GetFileName(path)}";
        GameVersionName = $"{sav.Version} (Gen {sav.Generation})";
        _trainerName = sav.OT;
        this.RaisePropertyChanged(nameof(TrainerName));

        TrainerID = sav.DisplayTID.ToString();
        SecretID = sav.DisplaySID.ToString();

        ZASaveUtil.EnsureBlockTypes(sav);

        _money = sav.Money;
        this.RaisePropertyChanged(nameof(Money));

        PlayTime = sav.PlayTimeString;

        // Populate Box Names
        BoxNames.Clear();
        var names = BoxUtil.GetBoxNames(sav);
        foreach (var name in names)
        {
            BoxNames.Add(name);
        }

        CurrentBoxIndex = Math.Clamp(sav.CurrentBox, 0, Math.Max(0, sav.BoxCount - 1));
        UpdateBoxDetails();

        // Load initial Pokémon into Editor (e.g. Box 1 Slot 1 or template)
        PKM? initialPkm = null;
        if (sav.HasBox && sav.BoxCount > 0)
        {
            var box0 = sav.GetBoxData(0);
            initialPkm = box0.FirstOrDefault(p => p.Species != 0) ?? box0.FirstOrDefault();
        }
        if (initialPkm == null || initialPkm.Species == 0)
            initialPkm = sav.BlankPKM;

        Editor.PopulateFields(initialPkm);
        BoxViewer.LoadSave(sav, CurrentBoxIndex);

        this.RaisePropertyChanged(nameof(HasInventory));
        this.RaisePropertyChanged(nameof(HasTrainerInfo));
        this.RaisePropertyChanged(nameof(HasParty));
        this.RaisePropertyChanged(nameof(HasDaycare));
        this.RaisePropertyChanged(nameof(HasPokedex));
        this.RaisePropertyChanged(nameof(HasEventFlags));
        this.RaisePropertyChanged(nameof(HasMysteryGift));
        this.RaisePropertyChanged(nameof(HasHallOfFame));
        this.RaisePropertyChanged(nameof(HasDonuts));
        this.RaisePropertyChanged(nameof(HasFashion));
        this.RaisePropertyChanged(nameof(HasBlockInspector));
        this.RaisePropertyChanged(nameof(IsZA));
        this.RaisePropertyChanged(nameof(BoxOccupancy));
        this.RaisePropertyChanged(nameof(FormattedMoney));
        this.RaisePropertyChanged(nameof(SaveFileName));
        StatusMessage = $"Loaded {sav.Version} save ({sav.BoxCount} boxes, {sav.PartyCount} party members).";
        return true;
    }

    public void NextBox()
    {
        if (_saveFile != null && CurrentBoxIndex < _saveFile.BoxCount - 1)
            CurrentBoxIndex++;
    }

    public void PreviousBox()
    {
        if (_saveFile != null && CurrentBoxIndex > 0)
            CurrentBoxIndex--;
    }

    public void RefreshCurrentBox()
    {
        if (_saveFile != null)
        {
            BoxViewer.LoadBox(_saveFile, CurrentBoxIndex);
            this.RaisePropertyChanged(nameof(BoxOccupancy));
        }
    }

    private void UpdateBoxDetails()
    {
        if (_saveFile == null)
            return;

        this.RaisePropertyChanged(nameof(BoxOccupancy));

        try
        {
            BoxWallpaper = _saveFile.WallpaperImage(CurrentBoxIndex);
        }
        catch
        {
            BoxWallpaper = null;
        }
    }

    public async Task SaveCurrentSaveFileAsync(IStorageProvider storageProvider, bool saveAs = false)
    {
        if (_saveFile == null)
        {
            StatusMessage = "No save file loaded to save.";
            return;
        }

        string targetPath = SaveFilePath;
        if (saveAs || string.IsNullOrEmpty(targetPath))
        {
            var suggestedName = !string.IsNullOrEmpty(_saveFile.Metadata.FileName)
                ? _saveFile.Metadata.FileName
                : "main";

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Pokémon Save File",
                SuggestedFileName = suggestedName,
                FileTypeChoices =
                [
                    new FilePickerFileType("All Supported Saves (*.sav, *.dsv, main)")
                    {
                        Patterns = ["*.sav", "*.dsv", "main", "*.bin"]
                    },
                    new FilePickerFileType("All Files (*.*)") { Patterns = ["*.*"] }
                ]
            });

            if (file == null)
                return;

            targetPath = file.Path.LocalPath;
        }

        try
        {
            byte[] data = _saveFile.Write().ToArray();
            await File.WriteAllBytesAsync(targetPath, data);
            SaveFilePath = targetPath;
            WindowTitle = $"PKHeX - {_saveFile.Version} - {Path.GetFileName(targetPath)}";
            StatusMessage = $"Successfully saved {data.Length:N0} bytes to {Path.GetFileName(targetPath)}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }

    public async Task<bool> ShowdownImportPKMAsync(IClipboard? clipboard)
    {
        if (clipboard == null) return false;
        string? text = await clipboard.TryGetTextAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            StatusMessage = "Clipboard is empty.";
            return false;
        }

        var sets = BattleTemplateTeams.TryGetSets(text);
        var set = sets.FirstOrDefault();
        if (set == null || set.Species == 0)
        {
            StatusMessage = "No valid Showdown set found in clipboard.";
            return false;
        }

        var pk = Editor.PreparePKM();
        pk.ApplySetDetails(set);
        Editor.PopulateFields(pk);
        StatusMessage = $"Imported Showdown set: {pk.Nickname} ({(Species)pk.Species}).";
        return true;
    }

    public async Task<bool> ShowdownExportPKMAsync(IClipboard? clipboard)
    {
        if (clipboard == null) return false;
        var pk = Editor.PreparePKM();
        string text = ShowdownParsing.GetShowdownText(pk);
        await clipboard.SetTextAsync(text);
        StatusMessage = $"Copied {pk.Nickname} Showdown set to clipboard.";
        return true;
    }

    public async Task<bool> ShowdownExportPartyAsync(IClipboard? clipboard)
    {
        if (clipboard == null || _saveFile == null) return false;
        var party = _saveFile.PartyData.Where(p => p.Species != 0).ToList();
        if (party.Count == 0)
        {
            StatusMessage = "Party is empty.";
            return false;
        }

        string text = ShowdownParsing.GetShowdownSets(party, Environment.NewLine + Environment.NewLine);
        await clipboard.SetTextAsync(text);
        StatusMessage = $"Copied {party.Count} party Pokémon Showdown sets to clipboard.";
        return true;
    }

    public async Task<bool> ShowdownExportCurrentBoxAsync(IClipboard? clipboard)
    {
        if (clipboard == null || _saveFile == null) return false;
        var box = _saveFile.GetBoxData(_currentBoxIndex).Where(p => p.Species != 0).ToList();
        if (box.Count == 0)
        {
            StatusMessage = $"Box {_currentBoxIndex + 1} is empty.";
            return false;
        }

        string text = ShowdownParsing.GetShowdownSets(box, Environment.NewLine + Environment.NewLine);
        await clipboard.SetTextAsync(text);
        StatusMessage = $"Copied {box.Count} Pokémon from Box {_currentBoxIndex + 1} to clipboard.";
        return true;
    }

    public async Task DumpCurrentBoxAsync(IStorageProvider storageProvider)
    {
        if (_saveFile == null || !_saveFile.HasBox)
        {
            StatusMessage = "No save file with box storage loaded.";
            return;
        }

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = $"Select Destination Folder to Dump Box {_currentBoxIndex + 1}",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;
        string path = folders[0].Path.LocalPath;
        int count = BoxExport.Export(_saveFile, path, _currentBoxIndex, BoxExportSettings.Default);
        StatusMessage = $"Dumped {count} Pokémon from Box {_currentBoxIndex + 1} to {Path.GetFileName(path)}.";
    }

    public async Task DumpAllBoxesAsync(IStorageProvider storageProvider)
    {
        if (_saveFile == null || !_saveFile.HasBox)
        {
            StatusMessage = "No save file with box storage loaded.";
            return;
        }

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Destination Folder to Dump All Boxes",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;
        string path = folders[0].Path.LocalPath;
        int count = BoxExport.Export(_saveFile, path, BoxExportSettings.Default);
        StatusMessage = $"Dumped {count} Pokémon from all {_saveFile.BoxCount} boxes to {Path.GetFileName(path)}.";
    }

    public async Task LoadBoxesAsync(IStorageProvider storageProvider)
    {
        if (_saveFile == null || !_saveFile.HasBox)
        {
            StatusMessage = "No save file with box storage loaded.";
            return;
        }

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder Containing Pokémon Files to Load",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;
        string path = folders[0].Path.LocalPath;
        int loaded = _saveFile.LoadBoxes(path, out string result, _currentBoxIndex);
        RefreshCurrentBox();
        StatusMessage = loaded > 0 ? $"Loaded {loaded} Pokémon: {result}" : $"Box Load: {result}";
    }
}
