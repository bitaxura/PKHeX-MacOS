using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class PokedexItemViewModel : ReactiveObject
{
    private bool _isSeen;
    private bool _isCaught;
    private WriteableBitmap? _sprite;

    public ushort Species { get; }
    public string DexNumber { get; }
    public int SortOrder { get; }
    public string Name { get; }

    public bool IsSeen
    {
        get => _isSeen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSeen, value);
            // In Pokémon logic, caught implies seen
            if (!value && IsCaught)
                IsCaught = false;
        }
    }

    public bool IsCaught
    {
        get => _isCaught;
        set
        {
            this.RaiseAndSetIfChanged(ref _isCaught, value);
            // Catching implies seen
            if (value && !IsSeen)
                IsSeen = true;
        }
    }

    public WriteableBitmap? Sprite
    {
        get => _sprite;
        private set => this.RaiseAndSetIfChanged(ref _sprite, value);
    }

    public PokedexItemViewModel(ushort species, string name, string dexNumber, int sortOrder, bool seen, bool caught, EntityContext context)
    {
        Species = species;
        Name = name;
        DexNumber = dexNumber;
        SortOrder = sortOrder;
        _isSeen = seen;
        _isCaught = caught;

        try
        {
            var raw = SpriteUtil.GetSprite(species, 0, 0, 0, 0, false, Shiny.Never, context);
            _sprite = raw?.ToBitmap();
        }
        catch
        {
            _sprite = null;
        }
    }
}
