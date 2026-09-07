using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Core;

namespace PKHeX.Avalonia.Views;

public partial class MysteryGiftWindow : Window
{
    public MysteryGiftViewModel ViewModel => (MysteryGiftViewModel)DataContext!;

    public event Action<PKM>? PokemonReceived;

    public MysteryGiftWindow()
    {
        InitializeComponent();
    }

    private async void OnImportCardClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Wonder Card / Mystery Gift File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Mystery Gift Files (*.wc*, *.pgf, *.pcd, *.wb*)")
                {
                    Patterns = ["*.wc*", "*.pgf", "*.pcd", "*.wb*", "*.pgt"]
                },
                new FilePickerFileType("All Files (*.*)") { Patterns = ["*.*"] }
            ]
        });

        if (files.Count > 0)
        {
            var data = await File.ReadAllBytesAsync(files[0].Path.LocalPath);
            ViewModel.ImportCard(data);
        }
    }

    private async void OnExportCardClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCard == null || ViewModel.SelectedCard.IsEmpty)
            return;

        var gift = ViewModel.SelectedCard.Gift;
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Wonder Card File",
            SuggestedFileName = gift.FileName,
            FileTypeChoices =
            [
                new FilePickerFileType($"Wonder Card (*.{gift.Extension})") { Patterns = [$"*.{gift.Extension}"] },
                new FilePickerFileType("All Files (*.*)") { Patterns = ["*.*"] }
            ]
        });

        if (file != null)
        {
            await File.WriteAllBytesAsync(file.Path.LocalPath, gift.Write().ToArray());
            ViewModel.StatusMessage = $"Exported card to {Path.GetFileName(file.Path.LocalPath)}.";
        }
    }

    private void OnReceiveToEditorClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCard == null || ViewModel.SelectedCard.IsEmpty)
            return;

        var pkm = ViewModel.ConvertToPokemon(ViewModel.SelectedCard);
        if (pkm != null)
        {
            PokemonReceived?.Invoke(pkm);
        }
    }

    private void OnClearCardClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCard != null)
            ViewModel.ClearCard(ViewModel.SelectedCard);
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
