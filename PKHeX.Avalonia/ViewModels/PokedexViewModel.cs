using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class PokedexViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private string _filterText = string.Empty;
    private int _totalSeen;
    private int _totalCaught;

    public List<PokedexItemViewModel> AllEntries { get; } = [];
    public ObservableCollection<PokedexItemViewModel> FilteredEntries { get; } = [];

    public int MaxSpecies => AllEntries.Count;

    public int TotalSeen
    {
        get => _totalSeen;
        private set => this.RaiseAndSetIfChanged(ref _totalSeen, value);
    }

    public int TotalCaught
    {
        get => _totalCaught;
        private set => this.RaiseAndSetIfChanged(ref _totalCaught, value);
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

    public PokedexViewModel(SaveFile sav)
    {
        _saveFile = sav;
        var gameEntries = GameDexUtil.GetGameEntries(sav);

        for (int i = 0; i < gameEntries.Count; i++)
        {
            var g = gameEntries[i];
            bool seen = sav.GetSeen(g.Species);
            bool caught = sav.GetCaught(g.Species);

            var entry = new PokedexItemViewModel(g.Species, g.Name, g.DexNumber, i, seen, caught, sav.Context);
            entry.PropertyChanged += (s, e) => UpdateCounters();
            AllEntries.Add(entry);
        }

        ApplyFilter();
        UpdateCounters();
    }

    public void ApplyFilter()
    {
        FilteredEntries.Clear();
        string q = _filterText.Trim().ToLowerInvariant();
        foreach (var entry in AllEntries)
        {
            if (string.IsNullOrEmpty(q) ||
                entry.Name.ToLowerInvariant().Contains(q) ||
                entry.Species.ToString().Contains(q) ||
                entry.DexNumber.ToLowerInvariant().Contains(q))
            {
                FilteredEntries.Add(entry);
            }
        }
    }

    public void UpdateCounters()
    {
        TotalSeen = AllEntries.Count(e => e.IsSeen);
        TotalCaught = AllEntries.Count(e => e.IsCaught);
    }

    public void SeenAll()
    {
        foreach (var e in FilteredEntries)
            e.IsSeen = true;
        UpdateCounters();
    }

    public void SeenNone()
    {
        foreach (var e in FilteredEntries)
            e.IsSeen = false;
        UpdateCounters();
    }

    public void CaughtAll()
    {
        foreach (var e in FilteredEntries)
            e.IsCaught = true;
        UpdateCounters();
    }

    public void CaughtNone()
    {
        foreach (var e in FilteredEntries)
            e.IsCaught = false;
        UpdateCounters();
    }

    public void CompleteAll()
    {
        foreach (var e in AllEntries)
        {
            e.IsSeen = true;
            e.IsCaught = true;
        }
        UpdateCounters();

        if (_saveFile is SAV9ZA za)
            za.Blocks.Zukan.CompleteDex(false);
        else if (_saveFile is SAV9SV sv)
            sv.Blocks.Zukan.CompleteDex(false);
        else if (_saveFile is SAV8SWSH swsh)
            swsh.Blocks.Zukan.CompleteDex(false);
        else if (_saveFile is SAV7b gg)
        {
            for (ushort s = 1; s <= 151; s++)
            {
                gg.Blocks.Zukan.SetSeen(s, 0, true);
                gg.Blocks.Zukan.SetCaught(s, true);
            }
            gg.Blocks.Zukan.SetSeen(808, 0, true); gg.Blocks.Zukan.SetCaught(808, true);
            gg.Blocks.Zukan.SetSeen(809, 0, true); gg.Blocks.Zukan.SetCaught(809, true);
        }
    }

    public void Save()
    {
        if (_saveFile is SAV9ZA za)
        {
            var dex = za.Blocks.Zukan;
            foreach (var entry in AllEntries)
            {
                if (entry.IsCaught)
                {
                    dex.SetDexEntryAll(entry.Species, false);
                }
                else if (entry.IsSeen)
                {
                    dex.SetDexEntryAll(entry.Species, false);
                    var e = dex.GetEntry(entry.Species);
                    // keep seen, remove caught
                    for (byte f = 0; f < 32; f++)
                        e.SetIsFormCaught(f, false);
                }
                else
                {
                    var e = dex.GetEntry(entry.Species);
                    e.Clear();
                }
            }
        }
        else if (_saveFile is SAV9SV sv)
        {
            var dex = sv.Blocks.Zukan;
            foreach (var entry in AllEntries)
            {
                if (entry.IsCaught)
                {
                    dex.SetDexEntryAll(entry.Species, false);
                }
                else if (entry.IsSeen)
                {
                    dex.SetDexEntryAll(entry.Species, false);
                    if (dex.GetRevision() == 0)
                        dex.DexPaldea.SetAllCaught(entry.Species, false);
                    else
                        dex.DexKitakami.SetAllCaught(entry.Species, false);
                }
                else
                {
                    dex.ClearDexEntryAll(entry.Species);
                }
            }
        }
        else if (_saveFile is SAV8SWSH swsh)
        {
            var dex = swsh.Blocks.Zukan;
            var indexes = Zukan8.GetRawIndexes(PersonalTable.SWSH, dex.GetRevision(), Zukan8Index.TotalCount);
            var lookup = indexes.GroupBy(z => z.Species).ToDictionary(g => g.Key, g => g.First().Entry);
            foreach (var entry in AllEntries)
            {
                if (lookup.TryGetValue(entry.Species, out var zEntry))
                {
                    dex.SetCaught(zEntry, entry.IsCaught);
                    dex.SetSeenRegion(zEntry, 0, 0, entry.IsSeen);
                }
            }
        }
        else if (_saveFile is SAV8LA la)
        {
            var dex = la.Blocks.PokedexSave;
            foreach (var entry in AllEntries)
            {
                if (entry.IsCaught)
                {
                    dex.SetPokeHasBeenUpdated(entry.Species);
                    dex.SetPokeObtainFlags(entry.Species, 0, 1);
                    dex.SetPokeCaughtInWildFlags(entry.Species, 0, 1);
                    dex.SetPokeSeenInWildFlags(entry.Species, 0, 1);
                }
                else if (entry.IsSeen)
                {
                    dex.SetPokeHasBeenUpdated(entry.Species);
                    dex.SetPokeSeenInWildFlags(entry.Species, 0, 1);
                    dex.SetPokeObtainFlags(entry.Species, 0, 0);
                    dex.SetPokeCaughtInWildFlags(entry.Species, 0, 0);
                }
                else
                {
                    dex.SetPokeSeenInWildFlags(entry.Species, 0, 0);
                    dex.SetPokeObtainFlags(entry.Species, 0, 0);
                    dex.SetPokeCaughtInWildFlags(entry.Species, 0, 0);
                }
            }
        }
        else if (_saveFile is SAV7b gg)
        {
            var dex = gg.Blocks.Zukan;
            foreach (var entry in AllEntries)
            {
                dex.SetSeen(entry.Species, 0, entry.IsSeen);
                dex.SetCaught(entry.Species, entry.IsCaught);
            }
        }
        else
        {
            foreach (var entry in AllEntries)
            {
                _saveFile.SetSeen(entry.Species, entry.IsSeen);
                _saveFile.SetCaught(entry.Species, entry.IsCaught);
            }
        }

        if (_saveFile is SAV3 s3)
            s3.MirrorSeenFlags();

        _saveFile.State.Edited = true;
    }
}
