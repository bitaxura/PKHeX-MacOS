using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Core;

namespace PKHeX.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel => (MainViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        AddHandler(DragDrop.DropEvent, OnWindowDrop);
        AddHandler(DragDrop.DragOverEvent, OnWindowDragOver);

        BoxViewerControl.RequestEditorPkm = () => ViewModel.Editor.PreparePKM();
        EditorControl.LegalityReportRequested += OpenLegalityReport;
        EditorControl.ExportPKMRequested += async () => await ExportActivePKMAsync();
    }

    private void OnWindowDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnWindowDrop(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();
        if (files == null)
            return;

        var first = files.FirstOrDefault();
        if (first != null)
        {
            string localPath = first.Path.LocalPath;
            ViewModel.LoadFile(localPath);
        }
    }

    private async Task OpenFilePickerAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Pokémon Save File or PKM",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("All Supported Formats (*.sav, *.dsv, main, *.pk*, *.pb8)")
                {
                    Patterns = ["*.sav", "*.dsv", "main", "*.bin", "*.pk*", "*.pb*", "*.pa*"]
                },
                new FilePickerFileType("Save Files (*.sav, *.dsv, main)")
                {
                    Patterns = ["*.sav", "*.dsv", "main", "*.bin"]
                },
                new FilePickerFileType("Pokémon Entity Files (*.pk*, *.pb*, *.pa*)")
                {
                    Patterns = ["*.pk*", "*.pb*", "*.pa*"]
                },
                new FilePickerFileType("All Files (*.*)") { Patterns = ["*.*"] }
            ]
        });

        if (files.Count > 0)
        {
            string path = files[0].Path.LocalPath;
            ViewModel.LoadFile(path);
        }
    }

    public async void OnMenuOpenClick(object? sender, RoutedEventArgs e) => await OpenFilePickerAsync();
    public async void OnNativeMenuOpenClick(object? sender, EventArgs e) => await OpenFilePickerAsync();

    public async void OnMenuSaveClick(object? sender, RoutedEventArgs e) => await ViewModel.SaveCurrentSaveFileAsync(StorageProvider, saveAs: false);
    public async void OnNativeMenuSaveClick(object? sender, EventArgs e) => await ViewModel.SaveCurrentSaveFileAsync(StorageProvider, saveAs: false);

    public async void OnMenuSaveAsClick(object? sender, RoutedEventArgs e) => await ViewModel.SaveCurrentSaveFileAsync(StorageProvider, saveAs: true);
    public async void OnNativeMenuSaveAsClick(object? sender, EventArgs e) => await ViewModel.SaveCurrentSaveFileAsync(StorageProvider, saveAs: true);

    public void OnMenuExitClick(object? sender, RoutedEventArgs e) => Close();

    private void ExportShowdown()
    {
        if (ViewModel.SaveFile == null)
        {
            ViewModel.StatusMessage = "No save file loaded.";
            return;
        }

        try
        {
            var sav = ViewModel.SaveFile;
            var boxPkms = sav.GetBoxData(ViewModel.CurrentBoxIndex);
            var sb = new StringBuilder();
            foreach (var pk in boxPkms)
            {
                if (pk.Species != 0)
                {
                    sb.AppendLine(ShowdownParsing.GetShowdownText(pk));
                    sb.AppendLine();
                }
            }

            var showdownText = sb.ToString();
            if (Clipboard != null)
            {
                Clipboard.SetTextAsync(showdownText);
                ViewModel.StatusMessage = $"Copied Box {ViewModel.CurrentBoxIndex + 1} Showdown set to clipboard.";
            }
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Showdown export error: {ex.Message}";
        }
    }

    public void OnMenuExportShowdownClick(object? sender, RoutedEventArgs e) => ExportShowdown();
    public void OnNativeMenuExportShowdownClick(object? sender, EventArgs e) => ExportShowdown();

    public async Task ExportActivePKMAsync()
    {
        var pk = ViewModel.Editor.PreparePKM();
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Pokémon Entity File",
            SuggestedFileName = pk.FileName,
            FileTypeChoices =
            [
                new FilePickerFileType($"PKM Format (.{pk.Extension})") { Patterns = [$"*.{pk.Extension}"] },
                new FilePickerFileType("All Files (*.*)") { Patterns = ["*.*"] }
            ]
        });

        if (file != null)
        {
            await File.WriteAllBytesAsync(file.Path.LocalPath, pk.Data.ToArray());
            ViewModel.StatusMessage = $"Exported Pokémon to {Path.GetFileName(file.Path.LocalPath)}.";
        }
    }

    public async void OnExportPKMClick(object? sender, RoutedEventArgs? e) => await ExportActivePKMAsync();

    public void OnMenuAboutClick(object? sender, RoutedEventArgs e) => ShowAbout();
    public void OnNativeMenuAboutClick(object? sender, EventArgs e) => ShowAbout();

    private void ShowAbout()
    {
        ViewModel.StatusMessage = "PKHeX - Avalonia macOS Port (Native .NET 10.0)";
    }

    public void OnPreviousBoxClick(object? sender, RoutedEventArgs e) => ViewModel.PreviousBox();
    public void OnNextBoxClick(object? sender, RoutedEventArgs e) => ViewModel.NextBox();

    public void OnNativeOpenInventoryClick(object? sender, EventArgs e) => OpenInventory();
    public void OnOpenInventoryClick(object? sender, RoutedEventArgs e) => OpenInventory();

    private async void OpenInventory()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new InventoryWindow
        {
            DataContext = new InventoryViewModel(ViewModel.SaveFile)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenTrainerClick(object? sender, EventArgs e) => OpenTrainer();
    public void OnOpenTrainerClick(object? sender, RoutedEventArgs e) => OpenTrainer();

    private async void OpenTrainer()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new TrainerEditorWindow
        {
            DataContext = new TrainerEditorViewModel(ViewModel.SaveFile)
        };
        var result = await win.ShowDialog<bool>(this);
        if (result)
        {
            // Sync updated values to main header
            ViewModel.TrainerName = ViewModel.SaveFile.OT;
            ViewModel.Money = ViewModel.SaveFile.Money;
            ViewModel.PlayTime = $"{ViewModel.SaveFile.PlayedHours}:{ViewModel.SaveFile.PlayedMinutes:00}";
            ViewModel.StatusMessage = "Trainer Info updated.";
        }
    }

    public void OnNativeOpenPokedexClick(object? sender, EventArgs e) => OpenPokedex();
    public void OnOpenPokedexClick(object? sender, RoutedEventArgs e) => OpenPokedex();

    private async void OpenPokedex()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new PokedexWindow
        {
            DataContext = new PokedexViewModel(ViewModel.SaveFile)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenEventFlagsClick(object? sender, EventArgs e) => OpenEventFlags();
    public void OnOpenEventFlagsClick(object? sender, RoutedEventArgs e) => OpenEventFlags();

    private async void OpenEventFlags()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new EventFlagsWindow
        {
            DataContext = new EventFlagsViewModel(ViewModel.SaveFile)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenMysteryGiftClick(object? sender, EventArgs e) => OpenMysteryGift();
    public void OnOpenMysteryGiftClick(object? sender, RoutedEventArgs e) => OpenMysteryGift();

    private async void OpenMysteryGift()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new MysteryGiftWindow
        {
            DataContext = new MysteryGiftViewModel(ViewModel.SaveFile)
        };
        win.PokemonReceived += pkm =>
        {
            ViewModel.Editor.PopulateFields(pkm);
            ViewModel.StatusMessage = $"Loaded Mystery Gift {pkm.Species} to Editor.";
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenDatabaseClick(object? sender, EventArgs e) => OpenDatabase();
    public void OnOpenDatabaseClick(object? sender, RoutedEventArgs e) => OpenDatabase();

    private async void OpenDatabase()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new DatabaseWindow
        {
            DataContext = new DatabaseViewModel(ViewModel.SaveFile)
        };
        win.PokemonSelected += pkm =>
        {
            ViewModel.Editor.PopulateFields(pkm);
            ViewModel.StatusMessage = $"Loaded {pkm.Species} from Database into Editor.";
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenBatchEditorClick(object? sender, EventArgs e) => OpenBatchEditor();
    public void OnOpenBatchEditorClick(object? sender, RoutedEventArgs e) => OpenBatchEditor();

    private async void OpenBatchEditor()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new BatchEditorWindow
        {
            DataContext = new BatchEditorViewModel(ViewModel.SaveFile, ViewModel.CurrentBoxIndex)
        };
        var res = await win.ShowDialog<bool>(this);
        if (res)
        {
            ViewModel.RefreshCurrentBox();
            ViewModel.StatusMessage = "Batch modifications applied to save.";
        }
    }

    public void OnNativeOpenLegalityReportClick(object? sender, EventArgs e) => OpenLegalityReport();
    public void OnOpenLegalityReportClick(object? sender, RoutedEventArgs e) => OpenLegalityReport();

    private async void OpenLegalityReport()
    {
        var pkm = ViewModel.Editor.PreparePKM();
        var win = new LegalityReportWindow
        {
            DataContext = new LegalityReportViewModel(pkm)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenHallOfFameClick(object? sender, EventArgs e) => OpenHallOfFame();
    public void OnOpenHallOfFameClick(object? sender, RoutedEventArgs e) => OpenHallOfFame();

    private async void OpenHallOfFame()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new HallOfFameWindow
        {
            DataContext = new HallOfFameViewModel(ViewModel.SaveFile)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenSettingsClick(object? sender, EventArgs e) => OpenSettings();
    public void OnOpenSettingsClick(object? sender, RoutedEventArgs e) => OpenSettings();

    private async void OpenSettings()
    {
        var win = new SettingsWindow
        {
            DataContext = new SettingsViewModel()
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenAboutClick(object? sender, EventArgs e) => OpenAbout();
    public void OnOpenAboutClick(object? sender, RoutedEventArgs e) => OpenAbout();

    private async void OpenAbout()
    {
        var win = new AboutWindow();
        await win.ShowDialog(this);
    }

    public void OnNativeOpenDonutsClick(object? sender, EventArgs e) => OpenDonuts();
    public void OnOpenDonutsClick(object? sender, RoutedEventArgs e) => OpenDonuts();

    private async void OpenDonuts()
    {
        if (ViewModel.SaveFile is not SAV9ZA za)
            return;

        var win = new DonutWindow
        {
            DataContext = new DonutViewModel(za)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenFashionClick(object? sender, EventArgs e) => OpenFashion();
    public void OnOpenFashionClick(object? sender, RoutedEventArgs e) => OpenFashion();

    private async void OpenFashion()
    {
        if (ViewModel.SaveFile == null)
            return;

        var win = new FashionWindow
        {
            DataContext = new FashionViewModel(ViewModel.SaveFile)
        };
        await win.ShowDialog(this);
    }

    public void OnNativeOpenBlockEditorClick(object? sender, EventArgs e) => OpenBlockEditor();
    public void OnOpenBlockEditorClick(object? sender, RoutedEventArgs e) => OpenBlockEditor();

    private async void OpenBlockEditor()
    {
        if (ViewModel.SaveFile is not ISCBlockArray blocks)
            return;

        var win = new BlockEditorWindow
        {
            DataContext = new BlockEditorViewModel(blocks)
        };
        await win.ShowDialog(this);
    }
}
