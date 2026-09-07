using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class EventFlagsWindow : Window
{
    public EventFlagsViewModel ViewModel => (EventFlagsViewModel)DataContext!;

    public EventFlagsWindow()
    {
        InitializeComponent();
    }

    private void OnAllFlagsOnClick(object? sender, RoutedEventArgs e) => ViewModel.SetAllFlags(true);
    private void OnAllFlagsOffClick(object? sender, RoutedEventArgs e) => ViewModel.SetAllFlags(false);

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
