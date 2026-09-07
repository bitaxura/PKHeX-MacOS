using Avalonia.Media.Imaging;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class InventoryItemViewModel : ReactiveObject
{
    private int _itemIndex;
    private string _itemName = string.Empty;
    private int _count;
    private int _maxCount = 999;
    private WriteableBitmap? _icon;

    public int ItemIndex
    {
        get => _itemIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _itemIndex, value);
            UpdateIcon();
        }
    }

    public string ItemName
    {
        get => _itemName;
        set => this.RaiseAndSetIfChanged(ref _itemName, value);
    }

    public int Count
    {
        get => _count;
        set => this.RaiseAndSetIfChanged(ref _count, value);
    }

    public int MaxCount
    {
        get => _maxCount;
        set => this.RaiseAndSetIfChanged(ref _maxCount, value);
    }

    public WriteableBitmap? Icon
    {
        get => _icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    public InventoryItemViewModel(int itemIndex, string itemName, int count, int maxCount)
    {
        _itemIndex = itemIndex;
        _itemName = itemName;
        _count = count;
        _maxCount = maxCount;
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        try
        {
            var raw = SpriteUtil.GetItemSprite(_itemIndex);
            Icon = raw?.ToBitmap();
        }
        catch
        {
            Icon = null;
        }
    }
}
