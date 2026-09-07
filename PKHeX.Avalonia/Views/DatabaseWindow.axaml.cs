using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Core;

namespace PKHeX.Avalonia.Views;

public partial class DatabaseWindow : Window
{
    public DatabaseViewModel ViewModel => (DatabaseViewModel)DataContext!;

    public event Action<PKM>? PokemonSelected;

    public DatabaseWindow()
    {
        InitializeComponent();
    }

    private void OnLoadToEditorClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedEntry != null)
        {
            PokemonSelected?.Invoke(ViewModel.SelectedEntry.Entity);
            Close(true);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
