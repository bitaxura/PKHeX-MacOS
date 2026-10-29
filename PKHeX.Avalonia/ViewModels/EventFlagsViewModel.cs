using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly EventWorkspace<IEventFlag37, ushort>? _workspace;
    private readonly IEventFlagArray? _flagArray;
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

        if (sav is SAV9ZA za)
        {
            var flagStorage = za.Blocks.Event;
            int flagCount = Math.Min(flagStorage.Count, 2000);
            for (int i = 0; i < flagCount; i++)
            {
                bool isSet = flagStorage.GetValue(i);
                AllFlags.Add(new EventFlagItemViewModel(i, $"Event Flag {i}", isSet));
            }

            var workStorage = za.Blocks.Work;
            int workCount = Math.Min(workStorage.Count, 1000);
            for (int i = 0; i < workCount; i++)
            {
                int val = (int)workStorage.GetValue(i);
                AllWorkValues.Add(new EventWorkItemViewModel(i, $"Work Value {i}", val));
            }
        }
        else if (sav is SAV8BS bs)
        {
            var obj = bs.FlagWork;
            string prefix = bs.Version == GameVersion.BD ? "bd" : "sp";
            var editor = new EventLabelCollectionSystem(prefix, obj.CountFlag - 1, obj.CountSystem - 1, obj.CountWork - 1);
            var flagDict = new System.Collections.Generic.Dictionary<int, string>();
            foreach (var item in editor.Flag)
                flagDict.TryAdd(item.Index, item.Name);
            var workDict = new System.Collections.Generic.Dictionary<int, string>();
            foreach (var item in editor.Work)
                workDict.TryAdd(item.Index, item.Name);

            for (int i = 0; i < obj.CountFlag; i++)
            {
                string name = flagDict.TryGetValue(i, out var label) ? label : $"Flag {i}";
                AllFlags.Add(new EventFlagItemViewModel(i, name, obj.GetFlag(i)));
            }

            for (int i = 0; i < obj.CountWork; i++)
            {
                string name = workDict.TryGetValue(i, out var label) ? label : $"Work {i}";
                AllWorkValues.Add(new EventWorkItemViewModel(i, name, (int)obj.GetWork(i)));
            }
        }
        else if (sav is SAV7b gg)
        {
            var ew = gg.EventWork;
            for (int i = 0; i < ew.CountFlag; i++)
                AllFlags.Add(new EventFlagItemViewModel(i, $"Flag {i}", ew.GetFlag(i)));

            for (int i = 0; i < ew.CountWork; i++)
                AllWorkValues.Add(new EventWorkItemViewModel(i, $"Work {i}", ew.GetWork(i)));
        }
        else
        {
            IEventFlag37? targetFlags = sav switch
            {
                IEventFlag37 f => f,
                IEventFlagProvider37 p => p.EventWork,
                _ => null,
            };

            if (targetFlags != null)
            {
                try
                {
                    _workspace = new EventWorkspace<IEventFlag37, ushort>(targetFlags, sav.Version);
                    var flagDict = new System.Collections.Generic.Dictionary<int, string>();
                    foreach (var item in _workspace.Labels.Flag)
                        flagDict.TryAdd(item.Index, item.Name);
                    var workDict = new System.Collections.Generic.Dictionary<int, string>();
                    foreach (var item in _workspace.Labels.Work)
                        workDict.TryAdd(item.Index, item.Name);

                    for (int i = 0; i < _workspace.Flags.Length; i++)
                    {
                        string name = flagDict.TryGetValue(i, out var label) ? label : $"Flag {i}";
                        AllFlags.Add(new EventFlagItemViewModel(i, name, _workspace.Flags[i]));
                    }

                    for (int i = 0; i < _workspace.Values.Length; i++)
                    {
                        string name = workDict.TryGetValue(i, out var label) ? label : $"Constant {i}";
                        AllWorkValues.Add(new EventWorkItemViewModel(i, name, _workspace.Values[i]));
                    }
                }
                catch
                {
                    _flagArray = targetFlags;
                    LoadGenericFlags();
                }
            }
            else if (sav is IEventFlagArray efa)
            {
                _flagArray = efa;
                LoadGenericFlags();
            }
        }

        ApplyFlagFilter();
        ApplyWorkFilter();
    }

    private void LoadGenericFlags()
    {
        if (_flagArray != null)
        {
            for (int i = 0; i < _flagArray.EventFlagCount; i++)
                AllFlags.Add(new EventFlagItemViewModel(i, $"Flag {i}", _flagArray.GetEventFlag(i)));
        }

        if (_saveFile is IEventWorkArray<ushort> ewu)
        {
            for (int i = 0; i < ewu.EventWorkCount; i++)
                AllWorkValues.Add(new EventWorkItemViewModel(i, $"Constant {i}", ewu.GetWork(i)));
        }
        else if (_saveFile is IEventWorkArray<byte> ewb)
        {
            for (int i = 0; i < ewb.EventWorkCount; i++)
                AllWorkValues.Add(new EventWorkItemViewModel(i, $"Work {i}", ewb.GetWork(i)));
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
        if (_workspace != null)
        {
            foreach (var f in AllFlags)
            {
                if (f.Index < _workspace.Flags.Length)
                    _workspace.Flags[f.Index] = f.IsSet;
            }
            foreach (var w in AllWorkValues)
            {
                if (w.Index < _workspace.Values.Length)
                    _workspace.Values[w.Index] = (ushort)w.Value;
            }
            _workspace.Save();
        }
        else if (_saveFile is SAV9ZA za)
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
        else if (_saveFile is SAV8BS bs)
        {
            var obj = bs.FlagWork;
            foreach (var f in AllFlags)
            {
                if (f.Index < obj.CountFlag)
                    obj.SetFlag(f.Index, f.IsSet);
            }
            foreach (var w in AllWorkValues)
            {
                if (w.Index < obj.CountWork)
                    obj.SetWork(w.Index, (ushort)w.Value);
            }
        }
        else if (_saveFile is SAV7b gg)
        {
            var ew = gg.EventWork;
            foreach (var f in AllFlags)
            {
                if (f.Index < ew.CountFlag)
                    ew.SetFlag(f.Index, f.IsSet);
            }
            foreach (var w in AllWorkValues)
            {
                if (w.Index < ew.CountWork)
                    ew.SetWork(w.Index, w.Value);
            }
        }
        else
        {
            if (_flagArray != null)
            {
                foreach (var f in AllFlags)
                    _flagArray.SetEventFlag(f.Index, f.IsSet);
            }

            if (_saveFile is IEventWorkArray<ushort> ewu)
            {
                foreach (var w in AllWorkValues)
                    ewu.SetWork(w.Index, (ushort)w.Value);
            }
            else if (_saveFile is IEventWorkArray<byte> ewb)
            {
                foreach (var w in AllWorkValues)
                    ewb.SetWork(w.Index, (byte)w.Value);
            }
        }

        _saveFile.State.Edited = true;
    }
}
