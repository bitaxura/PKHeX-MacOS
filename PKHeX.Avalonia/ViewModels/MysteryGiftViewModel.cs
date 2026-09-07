using System;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class MysteryGiftViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private readonly IMysteryGiftStorage? _storage;
    private MysteryGiftCardViewModel? _selectedCard;
    private string _statusMessage = string.Empty;

    public ObservableCollection<MysteryGiftCardViewModel> Cards { get; } = [];

    public MysteryGiftCardViewModel? SelectedCard
    {
        get => _selectedCard;
        set => this.RaiseAndSetIfChanged(ref _selectedCard, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool HasStorage => _storage != null;

    public MysteryGiftViewModel(SaveFile sav)
    {
        _saveFile = sav;
        if (sav is IMysteryGiftStorageProvider provider)
            _storage = provider.MysteryGiftStorage;

        if (_storage != null)
        {
            int count = _storage.GiftCountMax;
            for (int i = 0; i < count; i++)
            {
                var gift = _storage.GetMysteryGift(i);
                Cards.Add(new MysteryGiftCardViewModel(i, gift));
            }
        }

        SelectedCard = Cards.FirstOrDefault();
    }

    public bool ImportCard(byte[] data)
    {
        if (SelectedCard == null || _storage == null)
            return false;

        var gift = MysteryGift.GetMysteryGift(data);
        if (gift == null)
        {
            StatusMessage = "Invalid Mystery Gift file format.";
            return false;
        }

        SelectedCard.Gift = gift;
        StatusMessage = $"Imported '{gift.CardTitle}' into {SelectedCard.DisplayIndex}.";
        return true;
    }

    public void ClearCard(MysteryGiftCardViewModel card)
    {
        card.Gift.Clear();
        card.Gift = card.Gift; // trigger property refresh
        StatusMessage = $"Cleared {card.DisplayIndex}.";
    }

    public PKM? ConvertToPokemon(MysteryGiftCardViewModel card)
    {
        if (card.IsEmpty)
            return null;

        try
        {
            var pkm = card.Gift.ConvertToPKM(_saveFile);
            StatusMessage = $"Converted {card.Title} into {pkm.Species} entity.";
            return pkm;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error converting gift: {ex.Message}";
            return null;
        }
    }

    public void Save()
    {
        if (_storage != null)
        {
            foreach (var card in Cards)
            {
                _storage.SetMysteryGift(card.SlotIndex, card.Gift);
            }
            _saveFile.State.Edited = true;
        }
    }
}
