using System;
using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.Misc;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class DonutItemViewModel : ReactiveObject
{
    private Donut9a _donut;
    private byte _stars;
    private ushort _calories;
    private byte _levelBoost;
    private ushort _recipe;
    private ushort _berryName;
    private ushort _berry1;
    private ushort _berry2;
    private ushort _berry3;
    private ushort _berry4;
    private ushort _berry5;
    private ushort _berry6;
    private ushort _berry7;
    private ushort _berry8;
    private ulong _flavor0;
    private ulong _flavor1;
    private ulong _flavor2;

    public int Index { get; }
    public string DisplayIndex => $"#{Index + 1:000}";

    public Donut9a Donut => _donut;

    public string RecipeName
    {
        get
        {
            var names = GameInfo.Strings.donutName;
            return _recipe < names.Length ? names[_recipe] : $"Recipe #{_recipe}";
        }
    }

    public string Title => _donut.MillisecondsSince1970 == 0
        ? $"(Empty Slot #{Index + 1:000})"
        : $"{Stars}⭐ {RecipeName} @ {Calories} cal (+{LevelBoost} perk, {FlavorCount} powers)";

    public bool IsEmpty => _donut.MillisecondsSince1970 == 0;

    public byte Stars
    {
        get => _stars;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _stars, Math.Clamp(value, (byte)1, (byte)5));
            _donut.Stars = _stars;
            NotifyStats();
        }
    }

    public ushort Calories
    {
        get => _calories;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _calories, Math.Clamp(value, (ushort)0, (ushort)9999));
            _donut.Calories = _calories;
            NotifyStats();
        }
    }

    public byte LevelBoost
    {
        get => _levelBoost;
        set
        {
            Touch();
            // Donut Level Boost is a byte perk value (0-255) calculated from berry ingredients
            this.RaiseAndSetIfChanged(ref _levelBoost, Math.Clamp(value, (byte)0, (byte)255));
            _donut.LevelBoost = _levelBoost;
            NotifyStats();
        }
    }

    public ushort Recipe
    {
        get => _recipe;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _recipe, value);
            _donut.Donut = _recipe;
            NotifyStats();
        }
    }

    public ushort BerryName
    {
        get => _berryName;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berryName, value);
            _donut.BerryName = _berryName;
            this.RaisePropertyChanged(nameof(BerryNameIcon));
            NotifyStats();
        }
    }

    public ushort Berry1
    {
        get => _berry1;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry1, value);
            _donut.Berry1 = _berry1;
            this.RaisePropertyChanged(nameof(Berry1Icon));
            NotifyStats();
        }
    }

    public ushort Berry2
    {
        get => _berry2;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry2, value);
            _donut.Berry2 = _berry2;
            this.RaisePropertyChanged(nameof(Berry2Icon));
            NotifyStats();
        }
    }

    public ushort Berry3
    {
        get => _berry3;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry3, value);
            _donut.Berry3 = _berry3;
            this.RaisePropertyChanged(nameof(Berry3Icon));
            NotifyStats();
        }
    }

    public ushort Berry4
    {
        get => _berry4;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry4, value);
            _donut.Berry4 = _berry4;
            this.RaisePropertyChanged(nameof(Berry4Icon));
            NotifyStats();
        }
    }

    public ushort Berry5
    {
        get => _berry5;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry5, value);
            _donut.Berry5 = _berry5;
            this.RaisePropertyChanged(nameof(Berry5Icon));
            NotifyStats();
        }
    }

    public ushort Berry6
    {
        get => _berry6;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry6, value);
            _donut.Berry6 = _berry6;
            this.RaisePropertyChanged(nameof(Berry6Icon));
            NotifyStats();
        }
    }

    public ushort Berry7
    {
        get => _berry7;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry7, value);
            _donut.Berry7 = _berry7;
            this.RaisePropertyChanged(nameof(Berry7Icon));
            NotifyStats();
        }
    }

    public ushort Berry8
    {
        get => _berry8;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _berry8, value);
            _donut.Berry8 = _berry8;
            this.RaisePropertyChanged(nameof(Berry8Icon));
            NotifyStats();
        }
    }

    public ulong Flavor0
    {
        get => _flavor0;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _flavor0, value);
            _donut.Flavor0 = _flavor0;
            this.RaisePropertyChanged(nameof(Flavor0Icon));
            NotifyStats();
        }
    }

    public ulong Flavor1
    {
        get => _flavor1;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _flavor1, value);
            _donut.Flavor1 = _flavor1;
            this.RaisePropertyChanged(nameof(Flavor1Icon));
            NotifyStats();
        }
    }

    public ulong Flavor2
    {
        get => _flavor2;
        set
        {
            Touch();
            this.RaiseAndSetIfChanged(ref _flavor2, value);
            _donut.Flavor2 = _flavor2;
            this.RaisePropertyChanged(nameof(Flavor2Icon));
            NotifyStats();
        }
    }

    public int FlavorCount => _donut.FlavorCount;

    public WriteableBitmap? Sprite => _donut.Sprite()?.ToBitmap();

    public WriteableBitmap? BerryNameIcon => GetItemIcon(BerryName);
    public WriteableBitmap? Berry1Icon => GetItemIcon(Berry1);
    public WriteableBitmap? Berry2Icon => GetItemIcon(Berry2);
    public WriteableBitmap? Berry3Icon => GetItemIcon(Berry3);
    public WriteableBitmap? Berry4Icon => GetItemIcon(Berry4);
    public WriteableBitmap? Berry5Icon => GetItemIcon(Berry5);
    public WriteableBitmap? Berry6Icon => GetItemIcon(Berry6);
    public WriteableBitmap? Berry7Icon => GetItemIcon(Berry7);
    public WriteableBitmap? Berry8Icon => GetItemIcon(Berry8);

    public WriteableBitmap? Flavor0Icon => GetFlavorIcon(Flavor0);
    public WriteableBitmap? Flavor1Icon => GetFlavorIcon(Flavor1);
    public WriteableBitmap? Flavor2Icon => GetFlavorIcon(Flavor2);

    public string FlavorProfileText
    {
        get
        {
            if (IsEmpty) return "Empty Donut Slot";
            Span<int> flavors = stackalloc int[5];
            _donut.RecalculateDonutFlavors(flavors);
            return $"Spicy: {flavors[0]}  •  Fresh: {flavors[1]}  •  Sweet: {flavors[2]}  •  Bitter: {flavors[3]}  •  Sour: {flavors[4]}";
        }
    }

    public string DateTimeString
    {
        get
        {
            if (IsEmpty) return "Unbaked / Empty";
            try
            {
                var dt = Donut9a.Epoch.AddMilliseconds(_donut.MillisecondsSince1970);
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return "Unknown Date";
            }
        }
    }

    public DonutItemViewModel(int index, Donut9a donut)
    {
        Index = index;
        _donut = donut;
        LoadFromDonut();
    }

    private void LoadFromDonut()
    {
        _stars = _donut.Stars;
        _calories = _donut.Calories;
        _levelBoost = _donut.LevelBoost;
        _recipe = _donut.Donut;

        _berryName = _donut.BerryName;
        _berry1 = _donut.Berry1;
        _berry2 = _donut.Berry2;
        _berry3 = _donut.Berry3;
        _berry4 = _donut.Berry4;
        _berry5 = _donut.Berry5;
        _berry6 = _donut.Berry6;
        _berry7 = _donut.Berry7;
        _berry8 = _donut.Berry8;

        _flavor0 = _donut.Flavor0;
        _flavor1 = _donut.Flavor1;
        _flavor2 = _donut.Flavor2;
    }

    private void Touch()
    {
        if (_donut.MillisecondsSince1970 == 0)
        {
            var now = DateTime.Now;
            _donut.MillisecondsSince1970 = (ulong)(now - Donut9a.Epoch).TotalMilliseconds;
            _donut.UpdateDateTime();
        }
    }

    private void NotifyStats()
    {
        this.RaisePropertyChanged(nameof(Title));
        this.RaisePropertyChanged(nameof(IsEmpty));
        this.RaisePropertyChanged(nameof(RecipeName));
        this.RaisePropertyChanged(nameof(Sprite));
        this.RaisePropertyChanged(nameof(FlavorCount));
        this.RaisePropertyChanged(nameof(FlavorProfileText));
        this.RaisePropertyChanged(nameof(DateTimeString));
    }

    public void RecalculateStats()
    {
        Touch();
        _donut.RecalculateDonutStats();
        Refresh();
    }

    public void Clear()
    {
        _donut.Clear();
        Refresh();
    }

    public void Refresh()
    {
        LoadFromDonut();
        this.RaisePropertyChanged(nameof(Stars));
        this.RaisePropertyChanged(nameof(Calories));
        this.RaisePropertyChanged(nameof(LevelBoost));
        this.RaisePropertyChanged(nameof(Recipe));
        this.RaisePropertyChanged(nameof(BerryName));
        this.RaisePropertyChanged(nameof(Berry1));
        this.RaisePropertyChanged(nameof(Berry2));
        this.RaisePropertyChanged(nameof(Berry3));
        this.RaisePropertyChanged(nameof(Berry4));
        this.RaisePropertyChanged(nameof(Berry5));
        this.RaisePropertyChanged(nameof(Berry6));
        this.RaisePropertyChanged(nameof(Berry7));
        this.RaisePropertyChanged(nameof(Berry8));
        this.RaisePropertyChanged(nameof(Flavor0));
        this.RaisePropertyChanged(nameof(Flavor1));
        this.RaisePropertyChanged(nameof(Flavor2));

        this.RaisePropertyChanged(nameof(BerryNameIcon));
        this.RaisePropertyChanged(nameof(Berry1Icon));
        this.RaisePropertyChanged(nameof(Berry2Icon));
        this.RaisePropertyChanged(nameof(Berry3Icon));
        this.RaisePropertyChanged(nameof(Berry4Icon));
        this.RaisePropertyChanged(nameof(Berry5Icon));
        this.RaisePropertyChanged(nameof(Berry6Icon));
        this.RaisePropertyChanged(nameof(Berry7Icon));
        this.RaisePropertyChanged(nameof(Berry8Icon));

        this.RaisePropertyChanged(nameof(Flavor0Icon));
        this.RaisePropertyChanged(nameof(Flavor1Icon));
        this.RaisePropertyChanged(nameof(Flavor2Icon));

        NotifyStats();
    }

    private static WriteableBitmap? GetItemIcon(ushort itemID)
    {
        if (itemID == 0)
            return null;
        try
        {
            return SpriteUtil.GetItemSpriteA(itemID)?.ToBitmap();
        }
        catch
        {
            return null;
        }
    }

    private static WriteableBitmap? GetFlavorIcon(ulong hash)
    {
        if (hash == 0 || !DonutInfo.TryGetFlavorName(hash, out var name))
            return null;
        try
        {
            return DonutSpriteUtil.GetDonutFlavorImage(name)?.ToBitmap();
        }
        catch
        {
            return null;
        }
    }
}
