using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Core;

namespace PKHeX.Avalonia.Controls;

public partial class SlotControl : UserControl
{
    public static SlotViewModel? ActiveDraggedSlot { get; private set; }

    public SlotViewModel ViewModel => (SlotViewModel)DataContext!;

    public event Action<SlotViewModel>? SlotViewRequested;
    public event Action<SlotViewModel>? SlotSetRequested;
    public event Action<SlotViewModel>? SlotCloneRequested;
    public event Action<SlotViewModel>? SlotDeleteRequested;
    public event Action<SlotViewModel, SlotViewModel>? SlotSwapRequested;
    public event Action<SlotViewModel, PKM>? SlotFileDropped;

    public SlotControl()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private async void OnSlotPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            SlotViewRequested?.Invoke(ViewModel);

            if (ViewModel.HasPokemon)
            {
                ActiveDraggedSlot = ViewModel;
                var data = new DataTransfer();

                // Create temp file for macOS Finder drag-and-drop
                try
                {
                    string tempDir = Path.Combine(Path.GetTempPath(), "PKHeX_DragOut");
                    Directory.CreateDirectory(tempDir);
                    string fileName = ViewModel.PKM.FileName;
                    string filePath = Path.Combine(tempDir, fileName);
                    File.WriteAllBytes(filePath, ViewModel.PKM.Data.ToArray());

                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel?.StorageProvider != null)
                    {
                        var storageItem = await topLevel.StorageProvider.TryGetFileFromPathAsync(filePath);
                        if (storageItem != null)
                            data.Add(DataTransferItem.CreateFile(storageItem));
                    }
                }
                catch { }

                try
                {
                    await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Copy | DragDropEffects.Move);
                }
                finally
                {
                    ActiveDraggedSlot = null;
                }
            }
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        // Internal slot-to-slot drag
        if (ActiveDraggedSlot != null)
        {
            if (ActiveDraggedSlot != ViewModel)
                SlotSwapRequested?.Invoke(ActiveDraggedSlot, ViewModel);
            return;
        }

        // External file drag from Finder
        var files = e.DataTransfer.TryGetFiles();
        if (files != null && files.Length > 0)
        {
            var first = files.FirstOrDefault();
            if (first != null)
            {
                string localPath = first.Path.LocalPath;
                var obj = FileUtil.GetSupportedFile(localPath);
                if (obj is PKM pk)
                    SlotFileDropped?.Invoke(ViewModel, pk);
            }
        }
    }

    public void OnMenuViewClick(object? sender, RoutedEventArgs e) => SlotViewRequested?.Invoke(ViewModel);
    public void OnMenuSetClick(object? sender, RoutedEventArgs e) => SlotSetRequested?.Invoke(ViewModel);
    public void OnMenuCloneClick(object? sender, RoutedEventArgs e) => SlotCloneRequested?.Invoke(ViewModel);
    public void OnMenuDeleteClick(object? sender, RoutedEventArgs e) => SlotDeleteRequested?.Invoke(ViewModel);

    public async void OnMenuExportClick(object? sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasPokemon)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null)
            return;

        var pk = ViewModel.PKM;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Pokémon Slot",
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
        }
    }
}
