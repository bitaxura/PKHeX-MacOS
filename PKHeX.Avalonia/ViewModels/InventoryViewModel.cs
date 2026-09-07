using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class InventoryViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private readonly PlayerBag _bag;
    private readonly string[] _itemStrings;
    private InventoryPouchViewModel? _selectedPouch;
    private int _selectedNewItemId = 1;
    private int _newItemCount = 1;

    public ObservableCollection<InventoryPouchViewModel> Pouches { get; } = [];
    public ObservableCollection<ComboItem> AvailableItems { get; } = [];

    public InventoryPouchViewModel? SelectedPouch
    {
        get => _selectedPouch;
        set => this.RaiseAndSetIfChanged(ref _selectedPouch, value);
    }

    public int SelectedNewItemId
    {
        get => _selectedNewItemId;
        set => this.RaiseAndSetIfChanged(ref _selectedNewItemId, value);
    }

    public int NewItemCount
    {
        get => _newItemCount;
        set => this.RaiseAndSetIfChanged(ref _newItemCount, value);
    }

    public InventoryViewModel(SaveFile sav)
    {
        _saveFile = sav;
        _bag = sav.Inventory;
        _itemStrings = [.. GameInfo.Strings.GetItemStrings(sav.Context, sav.Version)];

        // Populate available items
        for (int i = 1; i < _itemStrings.Length; i++)
        {
            string name = _itemStrings[i];
            if (!string.IsNullOrWhiteSpace(name))
                AvailableItems.Add(new ComboItem(i, name));
        }

        // Populate pouches
        foreach (var pouch in _bag.Pouches)
        {
            Pouches.Add(new InventoryPouchViewModel(pouch, _itemStrings));
        }

        SelectedPouch = Pouches.FirstOrDefault();
    }

    public void AddSelectedItem()
    {
        if (SelectedPouch == null || SelectedNewItemId <= 0)
            return;

        string name = (SelectedNewItemId < _itemStrings.Length && !string.IsNullOrWhiteSpace(_itemStrings[SelectedNewItemId]))
            ? _itemStrings[SelectedNewItemId]
            : $"Item #{SelectedNewItemId}";

        SelectedPouch.AddItem(SelectedNewItemId, name, NewItemCount);
    }

    public void GiveAllCurrentPouch(int count = 999)
    {
        if (SelectedPouch == null)
            return;

        // Give legal items for this pouch
        var info = _bag.Info;
        var pouchType = SelectedPouch.PouchType;
        int maxCap = SelectedPouch.Pouch.Items.Length;

        for (int i = 1; i < _itemStrings.Length; i++)
        {
            if (SelectedPouch.Items.Count >= maxCap)
                break;

            if (info.IsLegal(pouchType, i, 1))
            {
                string name = _itemStrings[i];
                if (!string.IsNullOrWhiteSpace(name))
                    SelectedPouch.AddItem(i, name, Math.Min(count, SelectedPouch.Pouch.MaxCount));
            }
        }
    }

    public void Save()
    {
        foreach (var pouchVm in Pouches)
        {
            pouchVm.CommitToPouch();
        }
        _bag.CopyTo(_saveFile);
    }
}
