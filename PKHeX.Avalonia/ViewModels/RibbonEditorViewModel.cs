using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class RibbonItemViewModel : ReactiveObject
{
    public RibbonInfo Info { get; }
    public string Name { get; }
    public bool IsCount => Info.Type == RibbonValueType.Byte;
    public int MaxCount => Info.Type == RibbonValueType.Byte ? Info.MaxCount : 1;

    private bool _hasRibbon;
    public bool HasRibbon
    {
        get => _hasRibbon;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasRibbon, value);
            Info.HasRibbon = value;
        }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            this.RaiseAndSetIfChanged(ref _count, value);
            Info.RibbonCount = (byte)value;
            if (value > 0)
                HasRibbon = true;
            else if (IsCount && value == 0)
                HasRibbon = false;
        }
    }

    public RibbonItemViewModel(RibbonInfo info, string name)
    {
        Info = info;
        Name = name;
        _hasRibbon = info.HasRibbon;
        _count = info.RibbonCount;
    }
}

public sealed class RibbonEditorViewModel : ReactiveObject
{
    private readonly PKM _entity;
    private readonly List<RibbonItemViewModel> _allRibbons = [];
    private string _searchText = string.Empty;
    private string _ribbonSummary = string.Empty;

    public ObservableCollection<RibbonItemViewModel> FilteredRibbons { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            ApplyFilter();
        }
    }

    public string RibbonSummary
    {
        get => _ribbonSummary;
        private set => this.RaiseAndSetIfChanged(ref _ribbonSummary, value);
    }

    public RibbonEditorViewModel(PKM pk)
    {
        _entity = pk;
        var infoList = RibbonInfo.GetRibbonInfo(pk);
        var ribbonStrings = GameInfo.Strings.Ribbons;

        foreach (var info in infoList)
        {
            string cleanName = info.Name.StartsWith(RibbonInfo.PropertyPrefix)
                ? info.Name[RibbonInfo.PropertyPrefix.Length..]
                : info.Name;

            string displayName = ribbonStrings.GetName(info.Name);
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = cleanName;

            var item = new RibbonItemViewModel(info, displayName);
            item.PropertyChanged += (_, _) => UpdateSummary();
            _allRibbons.Add(item);
        }

        ApplyFilter();
        UpdateSummary();
    }

    public void ApplyFilter()
    {
        FilteredRibbons.Clear();
        string q = _searchText.Trim().ToLowerInvariant();

        foreach (var rib in _allRibbons)
        {
            if (string.IsNullOrEmpty(q) || rib.Name.ToLowerInvariant().Contains(q))
                FilteredRibbons.Add(rib);
        }
    }

    public void UpdateSummary()
    {
        int total = _allRibbons.Count;
        int active = _allRibbons.Count(r => r.HasRibbon || r.Count > 0);
        RibbonSummary = $"{active} / {total} Ribbons & Marks active";
    }

    public void GiveAll()
    {
        foreach (var rib in FilteredRibbons)
        {
            if (rib.IsCount)
                rib.Count = rib.MaxCount;
            else
                rib.HasRibbon = true;
        }
        UpdateSummary();
    }

    public void ClearAll()
    {
        foreach (var rib in FilteredRibbons)
        {
            if (rib.IsCount)
                rib.Count = 0;
            else
                rib.HasRibbon = false;
        }
        UpdateSummary();
    }

    public void Save()
    {
        foreach (var rib in _allRibbons)
        {
            if (rib.IsCount)
                ReflectUtil.SetValue(_entity, rib.Info.Name, (byte)rib.Count);
            else
                ReflectUtil.SetValue(_entity, rib.Info.Name, rib.HasRibbon);
        }
        _entity.RefreshChecksum();
    }
}

