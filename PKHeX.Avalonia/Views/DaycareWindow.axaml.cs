using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Core;

namespace PKHeX.Avalonia.Views;

public partial class DaycareWindow : Window
{
    public DaycareViewModel ViewModel => (DaycareViewModel)DataContext!;

    public event Action<PKM>? PokemonSelected;
    public Func<PKM>? RequestEditorPokemon;

    public DaycareWindow()
    {
        InitializeComponent();
    }

    private void OnViewSlotClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: DaycareSlotViewModel slot } && slot.HasPokemon)
        {
            PokemonSelected?.Invoke(slot.PKM);
            Close(true);
        }
    }

    private void OnDepositSlotClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: DaycareSlotViewModel slot } && RequestEditorPokemon != null)
        {
            var pk = RequestEditorPokemon();
            ViewModel.Deposit(slot.SlotIndex, pk);
        }
    }

    private void OnClearSlotClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: DaycareSlotViewModel slot })
        {
            ViewModel.Withdraw(slot.SlotIndex);
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(false);
}

