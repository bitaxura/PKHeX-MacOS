using System;
using System.Linq;
using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class MysteryGiftCardViewModel : ReactiveObject
{
    private DataMysteryGift _gift;
    private bool _giftUsed;
    private WriteableBitmap? _sprite;

    public int SlotIndex { get; }
    public string DisplayIndex => $"Card #{SlotIndex + 1}";

    public DataMysteryGift Gift
    {
        get => _gift;
        set
        {
            this.RaiseAndSetIfChanged(ref _gift, value);
            this.RaisePropertyChanged(nameof(Title));
            this.RaisePropertyChanged(nameof(IsEmpty));
            this.RaisePropertyChanged(nameof(Description));
            UpdateDisplay();
        }
    }

    public string Title => _gift.IsEmpty ? "(Empty Card Slot)" : (!string.IsNullOrWhiteSpace(_gift.CardTitle) ? _gift.CardTitle : $"Gift: {_gift.CardHeader}");

    public bool IsEmpty => _gift.IsEmpty;

    public string Description => _gift.IsEmpty
        ? "This slot is empty."
        : string.Join(Environment.NewLine, _gift.GetTextLines());

    public bool GiftUsed
    {
        get => _giftUsed;
        set
        {
            this.RaiseAndSetIfChanged(ref _giftUsed, value);
            _gift.GiftUsed = value;
        }
    }

    public WriteableBitmap? Sprite
    {
        get => _sprite;
        private set => this.RaiseAndSetIfChanged(ref _sprite, value);
    }

    public MysteryGiftCardViewModel(int slotIndex, DataMysteryGift gift)
    {
        SlotIndex = slotIndex;
        _gift = gift;
        _giftUsed = gift.GiftUsed;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (_gift.IsEmpty)
        {
            Sprite = null;
            return;
        }

        try
        {
            if (_gift.Species > 0)
            {
                var raw = SpriteUtil.GetSprite(_gift.Species, 0, 0, 0, 0, false, Shiny.Never, _gift.Context);
                Sprite = raw?.ToBitmap();
            }
            else if (_gift.ItemID > 0)
            {
                var raw = SpriteUtil.GetItemSprite(_gift.ItemID);
                Sprite = raw?.ToBitmap();
            }
            else
            {
                Sprite = null;
            }
        }
        catch
        {
            Sprite = null;
        }
    }
}
