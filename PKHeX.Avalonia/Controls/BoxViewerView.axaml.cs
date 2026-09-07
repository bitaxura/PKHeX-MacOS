using System;
using Avalonia.Controls;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Core;

namespace PKHeX.Avalonia.Controls;

public partial class BoxViewerView : UserControl
{
    public BoxViewerViewModel ViewModel => (BoxViewerViewModel)DataContext!;

    public Func<PKM>? RequestEditorPkm { get; set; }

    public BoxViewerView()
    {
        InitializeComponent();
    }

    public void OnSlotView(SlotViewModel slot)
    {
        ViewModel.SelectSlot(slot);
    }

    public void OnSlotSet(SlotViewModel slot)
    {
        if (RequestEditorPkm != null)
        {
            var pk = RequestEditorPkm();
            ViewModel.SetSlot(slot, pk);
        }
    }

    public void OnSlotClone(SlotViewModel slot)
    {
        if (!slot.HasPokemon)
            return;

        // Clone to next empty slot or duplicate
        for (int i = 0; i < ViewModel.BoxSlots.Count; i++)
        {
            var target = ViewModel.BoxSlots[i];
            if (!target.HasPokemon)
            {
                ViewModel.SetSlot(target, slot.PKM.Clone());
                break;
            }
        }
    }

    public void OnSlotDelete(SlotViewModel slot)
    {
        ViewModel.ClearSlot(slot);
    }

    public void OnSlotSwap(SlotViewModel a, SlotViewModel b)
    {
        ViewModel.SwapSlots(a, b);
    }

    public void OnSlotFileDropped(SlotViewModel slot, PKM pk)
    {
        ViewModel.SetSlot(slot, pk);
    }
}
