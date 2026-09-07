using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class HallOfFameWindow : Window
{
    public HallOfFameViewModel ViewModel => (HallOfFameViewModel)DataContext!;

    public HallOfFameWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
