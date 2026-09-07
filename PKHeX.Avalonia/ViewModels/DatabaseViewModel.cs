using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class DatabaseViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private string _searchText = string.Empty;
    private int _shinyFilter; // 0 All, 1 Shiny, 2 Non-Shiny
    private int _legalityFilter; // 0 All, 1 Legal, 2 Illegal
    private DatabaseEntryViewModel? _selectedEntry;
    private int _filteredCount;

    public List<DatabaseEntryViewModel> AllEntries { get; } = [];
    public ObservableCollection<DatabaseEntryViewModel> FilteredEntries { get; } = [];

    public int TotalCount => AllEntries.Count;

    public int FilteredCount
    {
        get => _filteredCount;
        private set => this.RaiseAndSetIfChanged(ref _filteredCount, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            ApplyFilter();
        }
    }

    public int ShinyFilter
    {
        get => _shinyFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _shinyFilter, value);
            ApplyFilter();
        }
    }

    public int LegalityFilter
    {
        get => _legalityFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _legalityFilter, value);
            ApplyFilter();
        }
    }

    public DatabaseEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public DatabaseViewModel(SaveFile sav)
    {
        _saveFile = sav;
        ScanSaveFile();
        ApplyFilter();
    }

    private void ScanSaveFile()
    {
        AllEntries.Clear();

        // Scan Party
        if (_saveFile.HasParty)
        {
            var party = _saveFile.PartyData;
            for (int i = 0; i < party.Count; i++)
            {
                var pk = party[i];
                if (pk.Species > 0)
                    AllEntries.Add(new DatabaseEntryViewModel(pk, -1, i));
            }
        }

        // Scan PC Boxes
        for (int b = 0; b < _saveFile.BoxCount; b++)
        {
            var box = _saveFile.GetBoxData(b);
            for (int s = 0; s < box.Length; s++)
            {
                var pk = box[s];
                if (pk.Species > 0)
                    AllEntries.Add(new DatabaseEntryViewModel(pk, b, s));
            }
        }
    }

    public void ApplyFilter()
    {
        FilteredEntries.Clear();
        string q = _searchText.Trim().ToLowerInvariant();

        foreach (var entry in AllEntries)
        {
            // Search text
            if (!string.IsNullOrEmpty(q))
            {
                bool match = entry.SpeciesName.ToLowerInvariant().Contains(q) ||
                             entry.Nickname.ToLowerInvariant().Contains(q) ||
                             entry.Species.ToString().Contains(q) ||
                             entry.Ability.ToLowerInvariant().Contains(q) ||
                             entry.Nature.ToLowerInvariant().Contains(q) ||
                             entry.Location.ToLowerInvariant().Contains(q);
                if (!match)
                    continue;
            }

            // Shiny filter
            if (_shinyFilter == 1 && !entry.IsShiny)
                continue;
            if (_shinyFilter == 2 && entry.IsShiny)
                continue;

            // Legality filter
            if (_legalityFilter == 1 && !entry.IsValid)
                continue;
            if (_legalityFilter == 2 && entry.IsValid)
                continue;

            FilteredEntries.Add(entry);
        }

        FilteredCount = FilteredEntries.Count;
    }
}
