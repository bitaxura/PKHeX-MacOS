using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class DatabaseEntryViewModel : ReactiveObject
{
    public PKM Entity { get; }
    public int BoxIndex { get; }
    public int SlotIndex { get; }

    public string Location => BoxIndex < 0 ? $"Party {SlotIndex + 1}" : $"Box {BoxIndex + 1}, Slot {SlotIndex + 1}";
    public ushort Species => Entity.Species;
    public string SpeciesName { get; }
    public string Nickname => Entity.Nickname;
    public int Level => Entity.CurrentLevel;
    public string Nature => ((Nature)Entity.Nature).ToString();
    public string Ability => GameInfo.Strings.Ability[Entity.Ability];
    public string HeldItem => GameInfo.Strings.GetItemStrings(Entity.Context, Entity.Version)[Entity.HeldItem];
    public bool IsShiny => Entity.IsShiny;
    public bool IsValid { get; }

    public string LegalityBadge => IsValid ? "✓ Legal" : "✕ Illegal";
    public string LegalityColor => IsValid ? "#2E7D32" : "#D32F2F";

    public WriteableBitmap? Sprite { get; }

    public DatabaseEntryViewModel(PKM pkm, int boxIndex, int slotIndex)
    {
        Entity = pkm;
        BoxIndex = boxIndex;
        SlotIndex = slotIndex;

        var strings = GameInfo.Strings;
        SpeciesName = (pkm.Species < strings.specieslist.Length && !string.IsNullOrEmpty(strings.specieslist[pkm.Species]))
            ? strings.specieslist[pkm.Species]
            : $"Species #{pkm.Species}";

        var la = new LegalityAnalysis(pkm);
        IsValid = la.Valid;

        try
        {
            var raw = SpriteUtil.GetSprite(pkm.Species, pkm.Form, pkm.Gender, 0, pkm.HeldItem, pkm.IsEgg, pkm.IsShiny ? Shiny.Always : Shiny.Never, pkm.Context);
            Sprite = raw?.ToBitmap();
        }
        catch
        {
            Sprite = null;
        }
    }
}
