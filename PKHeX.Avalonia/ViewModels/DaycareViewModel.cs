using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class DaycareSlotViewModel : ReactiveObject
{
    private PKM _pkm;
    private WriteableBitmap? _sprite;
    private string _displayName = "(Empty)";
    private string _levelInfo = "Level 0";
    private bool _hasPokemon;

    public int SlotIndex { get; }
    public string Title => $"Daycare Slot {SlotIndex + 1}";

    public PKM PKM
    {
        get => _pkm;
        private set => this.RaiseAndSetIfChanged(ref _pkm, value);
    }

    public WriteableBitmap? Sprite
    {
        get => _sprite;
        private set => this.RaiseAndSetIfChanged(ref _sprite, value);
    }

    public string DisplayName
    {
        get => _displayName;
        private set => this.RaiseAndSetIfChanged(ref _displayName, value);
    }

    public string LevelInfo
    {
        get => _levelInfo;
        private set => this.RaiseAndSetIfChanged(ref _levelInfo, value);
    }

    public bool HasPokemon
    {
        get => _hasPokemon;
        private set => this.RaiseAndSetIfChanged(ref _hasPokemon, value);
    }

    public DaycareSlotViewModel(int slotIndex, PKM pkm)
    {
        SlotIndex = slotIndex;
        _pkm = pkm;
        Update(pkm);
    }

    public void Update(PKM pk)
    {
        PKM = pk;
        HasPokemon = pk.Species != 0;

        if (!HasPokemon)
        {
            Sprite = null;
            DisplayName = "(Empty Slot)";
            LevelInfo = "Ready for deposit";
            return;
        }

        string spName = (pk.Species < GameInfo.Strings.Species.Count)
            ? GameInfo.Strings.Species[pk.Species]
            : $"Species {pk.Species}";

        DisplayName = $"{pk.Nickname} ({spName})";
        LevelInfo = $"Lv. {pk.CurrentLevel} • EXP: {pk.EXP:N0}";

        try
        {
            var raw = SpriteUtil.GetSprite(pk.Species, pk.Form, pk.Gender, 0, pk.HeldItem, pk.IsEgg, pk.IsShiny ? Shiny.Always : Shiny.Never, pk.Context);
            Sprite = raw?.ToBitmap();
        }
        catch
        {
            Sprite = null;
        }
    }
}

public sealed class DaycareViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private readonly IDaycareStorage? _daycare;
    private readonly IDaycareEggState? _eggState;
    private bool _hasEgg;
    private string _statusMessage = string.Empty;

    public ObservableCollection<DaycareSlotViewModel> Slots { get; } = [];

    public bool HasDaycare => _daycare != null && _daycare.DaycareSlotCount > 0;
    public bool HasEggState => _eggState != null;

    public bool HasEgg
    {
        get => _hasEgg;
        set => this.RaiseAndSetIfChanged(ref _hasEgg, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public DaycareViewModel(SaveFile sav)
    {
        _saveFile = sav;
        if (sav is IDaycareStorage dc)
        {
            _daycare = dc;
        }
        else if (sav is IDaycareMulti multi && multi.DaycareCount > 0)
        {
            _daycare = multi[0];
        }

        if (_daycare != null)
        {
            for (int i = 0; i < _daycare.DaycareSlotCount; i++)
            {
                var memory = _daycare.GetDaycareSlot(i);
                var pk = sav.GetStoredSlot(memory.Span);
                Slots.Add(new DaycareSlotViewModel(i, pk));
            }
        }

        if (sav is IDaycareEggState eggState)
        {
            _eggState = eggState;
            _hasEgg = eggState.IsEggAvailable;
        }
    }

    public void Deposit(int slotIndex, PKM pkm)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count)
            return;

        Slots[slotIndex].Update(pkm.Clone());
        StatusMessage = $"Deposited {pkm.Nickname} into Daycare Slot {slotIndex + 1}.";
    }

    public void Withdraw(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count)
            return;

        var blank = _saveFile.BlankPKM;
        Slots[slotIndex].Update(blank);
        StatusMessage = $"Withdrew Pokémon from Daycare Slot {slotIndex + 1}.";
    }

    public void Save()
    {
        if (_daycare != null)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                var pk = Slots[i].PKM;
                var memory = _daycare.GetDaycareSlot(i);
                _saveFile.SetBoxSlot(pk, memory.Span);
                _daycare.SetDaycareOccupied(i, pk.Species != 0);
            }
        }

        if (_eggState != null)
        {
            _eggState.IsEggAvailable = _hasEgg;
        }

        _saveFile.State.Edited = true;
    }
}
