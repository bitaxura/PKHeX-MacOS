using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class PokedexWindow : Window
{
    public PokedexViewModel ViewModel => (PokedexViewModel)DataContext!;

    public PokedexWindow()
    {
        InitializeComponent();
    }

    private void OnSeenAllClick(object? sender, RoutedEventArgs e) => ViewModel.SeenAll();
    private void OnSeenNoneClick(object? sender, RoutedEventArgs e) => ViewModel.SeenNone();
    private void OnCaughtAllClick(object? sender, RoutedEventArgs e) => ViewModel.CaughtAll();
    private void OnCaughtNoneClick(object? sender, RoutedEventArgs e) => ViewModel.CaughtNone();
    private void OnCompleteAllClick(object? sender, RoutedEventArgs e) => ViewModel.CompleteAll();

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
