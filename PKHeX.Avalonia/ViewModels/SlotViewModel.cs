using System;
using System.Text;
using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class SlotViewModel : ReactiveObject
{
    private PKM _pkm;
    private WriteableBitmap? _sprite;
    private bool _hasPokemon;
    private string _tooltipTitle = string.Empty;
    private string _tooltipSubtitle = string.Empty;
    private string _movesSummary = string.Empty;
    private string _statsSummary = string.Empty;
    private string _legalityText = "Legal";
    private string _legalityBadgeBrush = "#2E7D32";

    public int SlotIndex { get; }
    public bool IsParty { get; }

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

    public bool HasPokemon
    {
        get => _hasPokemon;
        private set => this.RaiseAndSetIfChanged(ref _hasPokemon, value);
    }

    public string TooltipTitle
    {
        get => _tooltipTitle;
        private set => this.RaiseAndSetIfChanged(ref _tooltipTitle, value);
    }

    public string TooltipSubtitle
    {
        get => _tooltipSubtitle;
        private set => this.RaiseAndSetIfChanged(ref _tooltipSubtitle, value);
    }

    public string MovesSummary
    {
        get => _movesSummary;
        private set => this.RaiseAndSetIfChanged(ref _movesSummary, value);
    }

    public string StatsSummary
    {
        get => _statsSummary;
        private set => this.RaiseAndSetIfChanged(ref _statsSummary, value);
    }

    public string LegalityText
    {
        get => _legalityText;
        private set => this.RaiseAndSetIfChanged(ref _legalityText, value);
    }

    public string LegalityBadgeBrush
    {
        get => _legalityBadgeBrush;
        private set => this.RaiseAndSetIfChanged(ref _legalityBadgeBrush, value);
    }

    public bool IsShiny => _hasPokemon && _pkm.IsShiny;
    public string GenderSymbol => !_hasPokemon ? "" : (_pkm.Gender == 0 ? "♂" : (_pkm.Gender == 1 ? "♀" : ""));
    public string GenderBrush => !_hasPokemon ? "#71717A" : (_pkm.Gender == 0 ? "#60A5FA" : (_pkm.Gender == 1 ? "#F472B6" : "#71717A"));
    public string LevelDisplay => !_hasPokemon ? "" : $"{_pkm.CurrentLevel}";
    public bool IsLegal => _hasPokemon && _legalityText == "Legal";
    public bool IsIllegal => _hasPokemon && _legalityText == "Illegal";

    public SlotViewModel(int slotIndex, bool isParty, PKM pkm, SaveFile sav)
    {
        SlotIndex = slotIndex;
        IsParty = isParty;
        _pkm = pkm;
        Update(pkm, sav);
    }

    public void Update(PKM pk, SaveFile sav)
    {
        PKM = pk;
        HasPokemon = pk.Species != 0;
        this.RaisePropertyChanged(nameof(IsShiny));
        this.RaisePropertyChanged(nameof(GenderSymbol));
        this.RaisePropertyChanged(nameof(GenderBrush));
        this.RaisePropertyChanged(nameof(LevelDisplay));
        this.RaisePropertyChanged(nameof(IsLegal));
        this.RaisePropertyChanged(nameof(IsIllegal));

        if (!HasPokemon)
        {
            Sprite = null;
            TooltipTitle = IsParty ? $"Party Slot {SlotIndex + 1} (Empty)" : $"Box Slot {SlotIndex + 1} (Empty)";
            TooltipSubtitle = "Click or drop a Pokémon here";
            MovesSummary = string.Empty;
            StatsSummary = string.Empty;
            LegalityText = "-";
            LegalityBadgeBrush = "#757575";
            return;
        }

        // Generate Sprite
        try
        {
            var raw = SpriteUtil.GetSprite(pk.Species, pk.Form, pk.Gender, 0, pk.HeldItem, pk.IsEgg, pk.IsShiny ? Shiny.Always : Shiny.Never, pk.Context);
            Sprite = raw.ToBitmap();
        }
        catch
        {
            Sprite = null;
        }

        // Identity
        string shinyMarker = pk.IsShiny ? "★ " : "";
        string genderMarker = pk.Gender switch { 0 => "♂", 1 => "♀", _ => "" };
        string speciesName = pk.Species < GameInfo.Strings.Species.Count ? GameInfo.Strings.Species[pk.Species] : $"Species {pk.Species}";
        TooltipTitle = $"{shinyMarker}{pk.Nickname} ({speciesName}) {genderMarker} Lv.{pk.CurrentLevel}";

        // Subtitle
        string natureName = ((int)pk.Nature < GameInfo.Strings.Natures.Count) ? GameInfo.Strings.Natures[(int)pk.Nature] : pk.Nature.ToString();
        string heldItemName = (pk.HeldItem < GameInfo.Strings.Item.Count && !string.IsNullOrEmpty(GameInfo.Strings.Item[pk.HeldItem]))
            ? GameInfo.Strings.Item[pk.HeldItem] : "None";
        string abilityName = (pk.Ability < GameInfo.Strings.Ability.Count && !string.IsNullOrEmpty(GameInfo.Strings.Ability[pk.Ability]))
            ? GameInfo.Strings.Ability[pk.Ability] : pk.Ability.ToString();
        TooltipSubtitle = $"Nature: {natureName} | Ability: {abilityName} | Item: {heldItemName}";

        // Moves
        var sbMoves = new StringBuilder();
        AppendMove(sbMoves, pk.Move1, pk.Move1_PP);
        AppendMove(sbMoves, pk.Move2, pk.Move2_PP);
        AppendMove(sbMoves, pk.Move3, pk.Move3_PP);
        AppendMove(sbMoves, pk.Move4, pk.Move4_PP);
        MovesSummary = sbMoves.ToString().TrimEnd();

        // Stats
        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);
        Span<int> evs = stackalloc int[6];
        pk.GetEVs(evs);
        StatsSummary = $"IV: {ivs[0]}/{ivs[1]}/{ivs[2]}/{ivs[3]}/{ivs[4]}/{ivs[5]}  |  EV: {evs[0]}/{evs[1]}/{evs[2]}/{evs[3]}/{evs[4]}/{evs[5]}";

        // Legality
        try
        {
            var la = new LegalityAnalysis(pk);
            if (la.Valid)
            {
                LegalityText = "Legal";
                LegalityBadgeBrush = "#2E7D32"; // Green
            }
            else
            {
                LegalityText = "Illegal";
                LegalityBadgeBrush = "#C62828"; // Red
            }
        }
        catch
        {
            LegalityText = "Unknown";
            LegalityBadgeBrush = "#757575";
        }
    }

    private static void AppendMove(StringBuilder sb, int moveId, int pp)
    {
        if (moveId <= 0 || moveId >= GameInfo.Strings.Move.Count)
            return;
        string name = GameInfo.Strings.Move[moveId];
        sb.AppendLine($"• {name} (PP: {pp})");
    }
}
