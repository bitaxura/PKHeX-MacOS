using System;
using System.Collections.ObjectModel;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class FashionViewModel : ReactiveObject
{
    private readonly SaveFile _sav;
    private string _statusMessage = string.Empty;

    public ObservableCollection<FashionCategoryViewModel> Categories { get; } = [];

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public FashionViewModel(SaveFile sav)
    {
        _sav = sav;
        LoadCategories();
    }

    private void LoadCategories()
    {
        Categories.Clear();

        if (_sav is SAV9ZA za)
        {
            var acc = za.Blocks;
            AddZACategory("👕 Tops", SaveBlockAccessor9ZA.KFashionTops, acc);
            AddZACategory("👖 Bottoms", SaveBlockAccessor9ZA.KFashionBottoms, acc);
            AddZACategory("🥋 All-in-One Outfits", SaveBlockAccessor9ZA.KFashionAllInOne, acc);
            AddZACategory("🧢 Headwear", SaveBlockAccessor9ZA.KFashionHeadwear, acc);
            AddZACategory("👓 Eyewear", SaveBlockAccessor9ZA.KFashionEyewear, acc);
            AddZACategory("🧤 Gloves", SaveBlockAccessor9ZA.KFashionGloves, acc);
            AddZACategory("🧦 Legwear", SaveBlockAccessor9ZA.KFashionLegwear, acc);
            AddZACategory("👟 Footwear", SaveBlockAccessor9ZA.KFashionFootwear, acc);
            AddZACategory("🎒 Satchels & Bags", SaveBlockAccessor9ZA.KFashionSatchels, acc);
            AddZACategory("💎 Earrings & Accessories", SaveBlockAccessor9ZA.KFashionEarrings, acc);

            AddZAHairCategory("💇 Hairstyle", SaveBlockAccessor9ZA.KHairMake00StyleHair, acc);
            AddZAHairCategory("💇 Bangs", SaveBlockAccessor9ZA.KHairMake01StyleBangs, acc);
            AddZAHairCategory("🎨 Hair Color", SaveBlockAccessor9ZA.KHairMake02ColorHair, acc);
            AddZAHairCategory("👁️ Eye Color", SaveBlockAccessor9ZA.KHairMake08ColorEyes, acc);
            AddZAHairCategory("💄 Lips & Makeup", SaveBlockAccessor9ZA.KHairMake11Lips, acc);
        }
    }

    private void AddZACategory(string name, uint key, SaveBlockAccessor9ZA acc)
    {
        if (!acc.HasBlock(key))
            return;

        var block = acc.GetBlock(key);
        var items = FashionItem9a.GetArray(block.Data);
        int unlocked = items.Count(z => z.IsOwned);
        Categories.Add(new FashionCategoryViewModel(name, key, unlocked, items.Length, false));
    }

    private void AddZAHairCategory(string name, uint key, SaveBlockAccessor9ZA acc)
    {
        if (!acc.HasBlock(key))
            return;

        var block = acc.GetBlock(key);
        var items = HairMakeItem9a.GetArray(block.Data);
        int unlocked = items.Count(z => z.IsNew);
        Categories.Add(new FashionCategoryViewModel(name, key, unlocked, items.Length, true));
    }

    public void UnlockAllFashion()
    {
        if (_sav is SAV9ZA za)
        {
            var acc = za.Blocks;
            foreach (var cat in Categories)
            {
                if (!acc.HasBlock(cat.BlockKey))
                    continue;

                var block = acc.GetBlock(cat.BlockKey);
                if (cat.IsHairOrMakeup)
                {
                    var items = HairMakeItem9a.GetArray(block.Data);
                    foreach (var item in items)
                        item.IsNew = true;
                    HairMakeItem9a.SetArray(items, block.Data);
                    cat.UnlockedCount = cat.TotalCount;
                }
                else
                {
                    FashionItem9a.ModifyAll(block.Data, z => z.IsOwned = true);
                    cat.UnlockedCount = cat.TotalCount;
                }
            }

            _sav.State.Edited = true;
            StatusMessage = "Successfully unlocked all clothing, hairstyles, and cosmetics!";
        }
    }

    public void Save()
    {
        _sav.State.Edited = true;
    }
}
