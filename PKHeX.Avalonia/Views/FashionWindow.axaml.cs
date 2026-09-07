using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class FashionWindow : Window
{
    public FashionViewModel ViewModel => (FashionViewModel)DataContext!;

    public FashionWindow()
    {
        InitializeComponent();
    }

    private void OnUnlockAllClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.UnlockAllFashion();
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
