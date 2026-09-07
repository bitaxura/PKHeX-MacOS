using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class FashionCategoryViewModel : ReactiveObject
{
    private int _unlockedCount;
    private int _totalCount;

    public string Name { get; }
    public uint BlockKey { get; }
    public bool IsHairOrMakeup { get; }

    public int UnlockedCount
    {
        get => _unlockedCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _unlockedCount, value);
            this.RaisePropertyChanged(nameof(StatusText));
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalCount, value);
            this.RaisePropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText => $"{UnlockedCount} / {TotalCount} Unlocked";

    public FashionCategoryViewModel(string name, uint blockKey, int unlocked, int total, bool isHair = false)
    {
        Name = name;
        BlockKey = blockKey;
        _unlockedCount = unlocked;
        _totalCount = total;
        IsHairOrMakeup = isHair;
    }
}
