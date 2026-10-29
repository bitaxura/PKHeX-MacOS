using System;
using System.Collections.ObjectModel;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class BoxViewerViewModel : ReactiveObject
{
    private int _currentBoxIndex;
    private string _currentBoxName = "Box 1";
    private SaveFile? _saveFile;
    private int _columns = 6;
    private int _rows = 5;
    private string _totalSlotsText = "30 Slots";
    private bool _hasParty = true;
    private bool _hasBox = true;

    public ObservableCollection<SlotViewModel> BoxSlots { get; } = [];
    public ObservableCollection<SlotViewModel> PartySlots { get; } = [];

    public Action<PKM>? OnSlotSelected { get; set; }

    public int CurrentBoxIndex
    {
        get => _currentBoxIndex;
        private set => this.RaiseAndSetIfChanged(ref _currentBoxIndex, value);
    }

    public string CurrentBoxName
    {
        get => _currentBoxName;
        private set => this.RaiseAndSetIfChanged(ref _currentBoxName, value);
    }

    public int Columns
    {
        get => _columns;
        private set => this.RaiseAndSetIfChanged(ref _columns, value);
    }

    public int Rows
    {
        get => _rows;
        private set => this.RaiseAndSetIfChanged(ref _rows, value);
    }

    public string TotalSlotsText
    {
        get => _totalSlotsText;
        private set => this.RaiseAndSetIfChanged(ref _totalSlotsText, value);
    }

    public bool HasParty
    {
        get => _hasParty;
        private set => this.RaiseAndSetIfChanged(ref _hasParty, value);
    }

    public bool HasBox
    {
        get => _hasBox;
        private set => this.RaiseAndSetIfChanged(ref _hasBox, value);
    }

    public void LoadSave(SaveFile sav, int boxIndex)
    {
        _saveFile = sav;
        HasBox = sav.HasBox;
        HasParty = sav.HasParty;
        LoadParty(sav);
        if (sav.HasBox)
            LoadBox(sav, boxIndex);
    }

    public void LoadBox(SaveFile sav, int boxIndex)
    {
        _saveFile = sav;
        CurrentBoxIndex = boxIndex;
        var names = BoxUtil.GetBoxNames(sav);
        CurrentBoxName = boxIndex < names.Length ? names[boxIndex] : $"Box {boxIndex + 1}";

        Columns = (sav.BoxSlotCount == 20) ? 5 : 6;
        Rows = (sav.BoxSlotCount == 20) ? 4 : 5;
        TotalSlotsText = $"{sav.BoxSlotCount} Slots";
        HasBox = sav.HasBox;

        if (!sav.HasBox)
        {
            BoxSlots.Clear();
            return;
        }

        var boxData = sav.GetBoxData(boxIndex);
        int slotCount = Math.Min(boxData.Length, sav.BoxSlotCount);

        if (BoxSlots.Count != slotCount)
        {
            BoxSlots.Clear();
            for (int i = 0; i < slotCount; i++)
            {
                BoxSlots.Add(new SlotViewModel(i, isParty: false, boxData[i], sav));
            }
        }
        else
        {
            for (int i = 0; i < slotCount; i++)
            {
                BoxSlots[i].Update(boxData[i], sav);
            }
        }
    }

    public void LoadParty(SaveFile sav)
    {
        _saveFile = sav;
        HasParty = sav.HasParty;
        if (!sav.HasParty)
        {
            PartySlots.Clear();
            return;
        }

        var partyData = sav.PartyData;
        int count = Math.Min(6, partyData.Count);

        if (PartySlots.Count != 6)
        {
            PartySlots.Clear();
            for (int i = 0; i < 6; i++)
            {
                var pk = i < count ? partyData[i] : sav.BlankPKM;
                PartySlots.Add(new SlotViewModel(i, isParty: true, pk, sav));
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                var pk = i < count ? partyData[i] : sav.BlankPKM;
                PartySlots[i].Update(pk, sav);
            }
        }
    }

    public void SelectSlot(SlotViewModel slot)
    {
        if (slot.HasPokemon)
            OnSlotSelected?.Invoke(slot.PKM);
    }

    public void SetSlot(SlotViewModel slot, PKM pk)
    {
        if (_saveFile == null)
            return;

        if (slot.IsParty)
        {
            if (_saveFile.HasParty)
            {
                _saveFile.SetPartySlotAtIndex(pk, slot.SlotIndex);
                slot.Update(pk, _saveFile);
            }
        }
        else
        {
            _saveFile.SetBoxSlotAtIndex(pk, CurrentBoxIndex, slot.SlotIndex);
            slot.Update(pk, _saveFile);
        }
    }

    public void ClearSlot(SlotViewModel slot)
    {
        if (_saveFile == null)
            return;

        var blank = _saveFile.BlankPKM;
        SetSlot(slot, blank);
    }

    public void SwapSlots(SlotViewModel source, SlotViewModel target)
    {
        if (_saveFile == null || source == target)
            return;

        var sourcePk = source.PKM.Clone();
        var targetPk = target.PKM.Clone();

        SetSlot(source, targetPk);
        SetSlot(target, sourcePk);
    }
}
