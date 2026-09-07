using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed record BerryOption(ushort ItemID, string Name);
public sealed record FlavorOption(ulong Hash, string Name);
public sealed record DonutRecipeOption(ushort ID, string Name);

public sealed class DonutViewModel : ReactiveObject
{
    private readonly SAV9ZA _sav;
    private readonly DonutPocket9a _donuts;
    private DonutItemViewModel? _selectedDonut;
    private string _statusMessage = string.Empty;
    private string _filterText = string.Empty;
    private bool _showOnlyOccupied;

    public List<DonutItemViewModel> AllDonuts { get; } = [];
    public ObservableCollection<DonutItemViewModel> FilteredDonuts { get; } = [];

    public List<BerryOption> BerryOptions { get; } = [];
    public List<FlavorOption> FlavorOptions { get; } = [];
    public List<DonutRecipeOption> DonutRecipeOptions { get; } = [];

    public DonutItemViewModel? SelectedDonut
    {
        get => _selectedDonut;
        set => this.RaiseAndSetIfChanged(ref _selectedDonut, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
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

    public bool ShowOnlyOccupied
    {
        get => _showOnlyOccupied;
        set
        {
            this.RaiseAndSetIfChanged(ref _showOnlyOccupied, value);
            ApplyFilter();
        }
    }

    public int OccupiedCount => AllDonuts.Count(d => !d.IsEmpty);
    public int TotalCount => AllDonuts.Count;

    public DonutViewModel(SAV9ZA sav)
    {
        _sav = sav;
        _donuts = sav.Blocks.Donuts;

        // Initialize Berry Options
        var items = GameInfo.Strings.itemlist;
        BerryOptions.Add(new BerryOption(0, "(None)"));
        foreach (var itemID in ItemStorage9ZA.Berry)
        {
            var name = (itemID < items.Length && !string.IsNullOrEmpty(items[itemID])) ? items[itemID] : $"Berry #{itemID}";
            BerryOptions.Add(new BerryOption(itemID, name));
        }

        // Initialize Flavor Options
        var flavorStrings = GameInfo.Strings.donutFlavor;
        FlavorOptions.Add(new FlavorOption(0, "(None)"));
        for (int i = 0; i < DonutInfo.Flavors.Count; i++)
        {
            var f = DonutInfo.Flavors[i];
            var name = (i < flavorStrings.Length && !string.IsNullOrEmpty(flavorStrings[i])) ? flavorStrings[i] : f.Name;
            FlavorOptions.Add(new FlavorOption(f.Hash, name));
        }

        // Initialize Donut Recipe Options
        var names = GameInfo.Strings.donutName;
        for (int i = 0; i < names.Length; i++)
        {
            DonutRecipeOptions.Add(new DonutRecipeOption((ushort)i, names[i]));
        }

        // Load all 999 slots
        for (int i = 0; i < DonutPocket9a.MaxCount; i++)
        {
            var entry = _donuts.GetDonut(i);
            AllDonuts.Add(new DonutItemViewModel(i, entry));
        }

        ApplyFilter();
        SelectedDonut = AllDonuts.FirstOrDefault(d => !d.IsEmpty) ?? AllDonuts.FirstOrDefault();
    }

    public void ApplyFilter()
    {
        FilteredDonuts.Clear();
        string q = _filterText.Trim().ToLowerInvariant();
        foreach (var item in AllDonuts)
        {
            if (_showOnlyOccupied && item.IsEmpty)
                continue;

            if (string.IsNullOrEmpty(q) ||
                item.DisplayIndex.ToLowerInvariant().Contains(q) ||
                item.Title.ToLowerInvariant().Contains(q) ||
                item.RecipeName.ToLowerInvariant().Contains(q))
            {
                FilteredDonuts.Add(item);
            }
        }

        if (SelectedDonut == null || !FilteredDonuts.Contains(SelectedDonut))
            SelectedDonut = FilteredDonuts.FirstOrDefault();

        this.RaisePropertyChanged(nameof(OccupiedCount));
    }

    public void RecalculateSelectedDonut()
    {
        if (SelectedDonut != null)
        {
            SelectedDonut.RecalculateStats();
            StatusMessage = $"Recalculated {SelectedDonut.DisplayIndex} stats from berry ingredients.";
            this.RaisePropertyChanged(nameof(OccupiedCount));
        }
    }

    public void CloneCurrentToAll()
    {
        if (SelectedDonut != null)
        {
            _donuts.CloneAllFromIndex(SelectedDonut.Index);
            RefreshAll();
            StatusMessage = $"Cloned {SelectedDonut.DisplayIndex} to all 999 donut slots!";
        }
    }

    public void RecalculateAll()
    {
        for (int i = 0; i < DonutPocket9a.MaxCount; i++)
        {
            var d = _donuts.GetDonut(i);
            if (d.MillisecondsSince1970 != 0)
                d.RecalculateDonutStats();
        }
        RefreshAll();
        StatusMessage = "Recalculated stats from berries for all occupied donuts!";
    }

    public void GiveAllShinyBoostDonuts()
    {
        _donuts.SetAllAsShinyTemplate();
        RefreshAll();
        StatusMessage = "All 999 donut slots set to 5⭐ Shiny Boost Template!";
    }

    public void GiveAllLevel3Donuts()
    {
        _donuts.SetAllRandomLv3();
        RefreshAll();
        StatusMessage = "All 999 donut slots set to Level 3 Power Donuts!";
    }

    public void ClearSelectedDonut()
    {
        if (SelectedDonut != null)
        {
            SelectedDonut.Clear();
            StatusMessage = $"Cleared {SelectedDonut.DisplayIndex}.";
            this.RaisePropertyChanged(nameof(OccupiedCount));
        }
    }

    public void ClearAllDonuts()
    {
        for (int i = 0; i < DonutPocket9a.MaxCount; i++)
        {
            _donuts.GetDonut(i).Data.Clear();
        }
        RefreshAll();
        StatusMessage = "All 999 donut slots cleared.";
    }

    private void RefreshAll()
    {
        foreach (var item in AllDonuts)
            item.Refresh();
        ApplyFilter();
    }

    public void Save()
    {
        _sav.State.Edited = true;
    }
}
