using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class BlockEditorWindow : Window
{
    public BlockEditorViewModel ViewModel => (BlockEditorViewModel)DataContext!;

    public BlockEditorWindow()
    {
        InitializeComponent();
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedBlock == null)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Block Binary",
            SuggestedFileName = $"{ViewModel.SelectedBlock.KeyHex}.bin",
            DefaultExtension = "bin"
        });

        if (file != null)
        {
            ViewModel.ExportSelectedBlock(file.Path.LocalPath);
        }
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedBlock == null)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Block Binary",
            AllowMultiple = false
        });

        if (files.Count > 0)
        {
            ViewModel.ImportSelectedBlock(files[0].Path.LocalPath);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
