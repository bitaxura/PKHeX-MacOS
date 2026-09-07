using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class InventoryWindow : Window
{
    public InventoryViewModel ViewModel => (InventoryViewModel)DataContext!;

    public InventoryWindow()
    {
        InitializeComponent();
    }

    private void OnAddItemClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.AddSelectedItem();
    }

    private void OnGiveAllClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.GiveAllCurrentPouch(999);
    }

    private void OnSortClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SelectedPouch?.SortItems();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.SelectedPouch?.ClearAll();
    }

    private void OnDeleteItemClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: InventoryItemViewModel item })
        {
            ViewModel.SelectedPouch?.RemoveItem(item);
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
