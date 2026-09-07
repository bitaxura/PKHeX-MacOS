using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class EventFlagItemViewModel : ReactiveObject
{
    private bool _isSet;

    public int Index { get; }
    public string DisplayIndex => $"#{Index:0000}";
    public string Name { get; }

    public bool IsSet
    {
        get => _isSet;
        set => this.RaiseAndSetIfChanged(ref _isSet, value);
    }

    public EventFlagItemViewModel(int index, string name, bool isSet)
    {
        Index = index;
        Name = name;
        _isSet = isSet;
    }
}

public sealed class EventWorkItemViewModel : ReactiveObject
{
    private int _value;

    public int Index { get; }
    public string DisplayIndex => $"#{Index:0000}";
    public string Name { get; }

    public int Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public EventWorkItemViewModel(int index, string name, int value)
    {
        Index = index;
        Name = name;
        _value = value;
    }
}

public sealed class EventFlagsViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private readonly IEventFlagArray? _flagArray;
    private readonly IEventWorkArray<ushort>? _workArray;
    private string _flagSearchText = string.Empty;
    private string _workSearchText = string.Empty;

    public List<EventFlagItemViewModel> AllFlags { get; } = [];
    public ObservableCollection<EventFlagItemViewModel> FilteredFlags { get; } = [];

    public List<EventWorkItemViewModel> AllWorkValues { get; } = [];
    public ObservableCollection<EventWorkItemViewModel> FilteredWorkValues { get; } = [];

    public bool HasWorkValues => AllWorkValues.Count > 0;

    public string FlagSearchText
    {
        get => _flagSearchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _flagSearchText, value);
            ApplyFlagFilter();
        }
    }

    public string WorkSearchText
    {
        get => _workSearchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _workSearchText, value);
            ApplyWorkFilter();
        }
    }

    public EventFlagsViewModel(SaveFile sav)
    {
        _saveFile = sav;

        if (sav is IEventFlagArray flags)
        {
            _flagArray = flags;
        }
        else if (sav is IEventFlagProvider37 prov)
        {
            _flagArray = prov.EventWork;
        }

        if (sav is IEventWorkArray<ushort> work)
        {
            _workArray = work;
        }
        else if (sav is IEventFlagProvider37 provWork)
        {
            _workArray = provWork.EventWork;
        }

        if (sav is SAV9ZA za)
        {
            var flagStorage = za.Blocks.Event;
            int flagCount = Math.Min(flagStorage.Count, 1000);
            for (int i = 0; i < flagCount; i++)
            {
                bool isSet = flagStorage.GetValue(i);
                AllFlags.Add(new EventFlagItemViewModel(i, $"Event Flag {i}", isSet));
            }

            var workStorage = za.Blocks.Work;
            int workCount = Math.Min(workStorage.Count, 500);
            for (int i = 0; i < workCount; i++)
            {
                int val = (int)workStorage.GetValue(i);
                AllWorkValues.Add(new EventWorkItemViewModel(i, $"Work Value {i}", val));
            }
        }
        else
        {
            LoadData();
        }

        ApplyFlagFilter();
        ApplyWorkFilter();
    }

    private void LoadData()
    {
        if (_flagArray != null)
        {
            int count = _flagArray.EventFlagCount;
            for (int i = 0; i < count; i++)
            {
                bool isSet = _flagArray.GetEventFlag(i);
                AllFlags.Add(new EventFlagItemViewModel(i, $"Flag {i}", isSet));
            }
        }

        if (_workArray != null)
        {
            int count = _workArray.EventWorkCount;
            for (int i = 0; i < count; i++)
            {
                int val = _workArray.GetWork(i);
                AllWorkValues.Add(new EventWorkItemViewModel(i, $"Constant {i}", val));
            }
        }
    }

    public void ApplyFlagFilter()
    {
        FilteredFlags.Clear();
        string q = _flagSearchText.Trim().ToLowerInvariant();
        foreach (var f in AllFlags)
        {
            if (string.IsNullOrEmpty(q) || f.Index.ToString().Contains(q) || f.Name.ToLowerInvariant().Contains(q))
                FilteredFlags.Add(f);
        }
    }

    public void ApplyWorkFilter()
    {
        FilteredWorkValues.Clear();
        string q = _workSearchText.Trim().ToLowerInvariant();
        foreach (var w in AllWorkValues)
        {
            if (string.IsNullOrEmpty(q) || w.Index.ToString().Contains(q) || w.Name.ToLowerInvariant().Contains(q))
                FilteredWorkValues.Add(w);
        }
    }

    public void SetAllFlags(bool value)
    {
        foreach (var f in FilteredFlags)
            f.IsSet = value;
    }

    public void Save()
    {
        if (_saveFile is SAV9ZA za)
        {
            var flagStorage = za.Blocks.Event;
            foreach (var f in AllFlags)
            {
                if (f.Index < flagStorage.Count)
                    flagStorage.SetValue(f.Index, f.IsSet);
            }

            var workStorage = za.Blocks.Work;
            foreach (var w in AllWorkValues)
            {
                if (w.Index < workStorage.Count)
                    workStorage.SetValue(w.Index, (ulong)w.Value);
            }
        }
        else
        {
            if (_flagArray != null)
            {
                foreach (var f in AllFlags)
                    _flagArray.SetEventFlag(f.Index, f.IsSet);
            }

            if (_workArray != null)
            {
                foreach (var w in AllWorkValues)
                    _workArray.SetWork(w.Index, (ushort)w.Value);
            }
        }

        _saveFile.State.Edited = true;
    }
}
