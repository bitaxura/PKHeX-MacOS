using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class InventoryPouchViewModel : ReactiveObject
{
    private readonly InventoryPouch _pouch;
    private string _pouchName = string.Empty;
    private int _itemCount;

    public InventoryType PouchType => _pouch.Type;
    public InventoryPouch Pouch => _pouch;

    public string PouchName
    {
        get => _pouchName;
        private set => this.RaiseAndSetIfChanged(ref _pouchName, value);
    }

    public int ItemCount
    {
        get => _itemCount;
        private set => this.RaiseAndSetIfChanged(ref _itemCount, value);
    }

    public ObservableCollection<InventoryItemViewModel> Items { get; } = [];

    public InventoryPouchViewModel(InventoryPouch pouch, string[] itemStrings)
    {
        _pouch = pouch;
        _pouchName = GetFriendlyPouchName(pouch.Type);

        LoadItems(itemStrings);
    }

    public void LoadItems(string[] itemStrings)
    {
        Items.Clear();
        foreach (var itm in _pouch.Items)
        {
            if (itm.Index <= 0 && itm.Count <= 0)
                continue;

            string name = (itm.Index >= 0 && itm.Index < itemStrings.Length && !string.IsNullOrEmpty(itemStrings[itm.Index]))
                ? itemStrings[itm.Index]
                : $"Item #{itm.Index}";

            Items.Add(new InventoryItemViewModel(itm.Index, name, itm.Count, _pouch.MaxCount));
        }
        UpdateCount();
    }

    public void AddItem(int itemIndex, string itemName, int count)
    {
        if (itemIndex <= 0)
            return;

        var existing = Items.FirstOrDefault(i => i.ItemIndex == itemIndex);
        if (existing != null)
        {
            existing.Count = Math.Clamp(existing.Count + count, 1, existing.MaxCount);
        }
        else
        {
            Items.Add(new InventoryItemViewModel(itemIndex, itemName, Math.Clamp(count, 1, _pouch.MaxCount), _pouch.MaxCount));
        }
        UpdateCount();
    }

    public void RemoveItem(InventoryItemViewModel item)
    {
        Items.Remove(item);
        UpdateCount();
    }

    public void ClearAll()
    {
        Items.Clear();
        UpdateCount();
    }

    public void SortItems()
    {
        var sorted = Items.OrderBy(i => i.ItemName).ToList();
        Items.Clear();
        foreach (var item in sorted)
            Items.Add(item);
        UpdateCount();
    }

    public void UpdateCount()
    {
        ItemCount = Items.Count;
    }

    public void CommitToPouch()
    {
        var targetItems = _pouch.Items;
        int maxCapacity = targetItems.Length;

        for (int i = 0; i < maxCapacity; i++)
        {
            if (i < Items.Count)
            {
                targetItems[i].Index = Items[i].ItemIndex;
                targetItems[i].Count = Items[i].Count;
            }
            else
            {
                targetItems[i].Index = 0;
                targetItems[i].Count = 0;
            }
        }
    }

    private static string GetFriendlyPouchName(InventoryType type) => type switch
    {
        InventoryType.Items => "Items",
        InventoryType.KeyItems => "Key Items",
        InventoryType.TMHMs => "TMs & HMs",
        InventoryType.Medicine => "Medicine",
        InventoryType.Berries => "Berries",
        InventoryType.Balls => "Poké Balls",
        InventoryType.BattleItems => "Battle Items",
        InventoryType.MailItems => "Mail",
        InventoryType.PCItems => "PC Items",
        InventoryType.FreeSpace => "Free Space",
        InventoryType.ZCrystals => "Z-Crystals",
        InventoryType.Candy => "Candy",
        InventoryType.Treasure => "Treasure",
        InventoryType.Ingredients => "Ingredients",
        InventoryType.MegaStones => "Mega Stones",
        _ => type.ToString()
    };
}
