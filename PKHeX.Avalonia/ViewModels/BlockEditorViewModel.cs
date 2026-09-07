using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class BlockEditorViewModel : ReactiveObject
{
    private readonly ISCBlockArray _sav;
    private readonly List<BlockItemViewModel> _allBlocks = [];
    private BlockItemViewModel? _selectedBlock;
    private string _filterText = string.Empty;
    private string _statusMessage = string.Empty;

    public ObservableCollection<BlockItemViewModel> FilteredBlocks { get; } = [];

    public BlockItemViewModel? SelectedBlock
    {
        get => _selectedBlock;
        set => this.RaiseAndSetIfChanged(ref _selectedBlock, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterText, value);
            ApplyFilter();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public BlockEditorViewModel(ISCBlockArray sav)
    {
        _sav = sav;
        foreach (var block in sav.AllBlocks)
        {
            _allBlocks.Add(new BlockItemViewModel(block));
        }

        ApplyFilter();
        SelectedBlock = FilteredBlocks.FirstOrDefault();
    }

    private void ApplyFilter()
    {
        FilteredBlocks.Clear();
        string q = _filterText.Trim().ToLowerInvariant();
        foreach (var item in _allBlocks)
        {
            if (string.IsNullOrEmpty(q) || item.KeyHex.ToLowerInvariant().Contains(q))
                FilteredBlocks.Add(item);
        }
    }

    public void ExportSelectedBlock(string path)
    {
        if (SelectedBlock == null)
            return;

        File.WriteAllBytes(path, SelectedBlock.Block.Data.ToArray());
        StatusMessage = $"Exported {SelectedBlock.KeyHex} ({SelectedBlock.Length} bytes) to {Path.GetFileName(path)}.";
    }

    public void ImportSelectedBlock(string path)
    {
        if (SelectedBlock == null)
            return;

        var data = File.ReadAllBytes(path);
        if (data.Length != SelectedBlock.Length)
        {
            StatusMessage = $"Cannot import: file size ({data.Length}) does not match block size ({SelectedBlock.Length}).";
            return;
        }

        data.CopyTo(SelectedBlock.Block.Data);
        StatusMessage = $"Successfully imported {data.Length} bytes into {SelectedBlock.KeyHex}.";
    }
}
