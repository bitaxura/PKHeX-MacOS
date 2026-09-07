using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class DonutWindow : Window
{
    public DonutViewModel ViewModel => (DonutViewModel)DataContext!;

    public DonutWindow()
    {
        InitializeComponent();
    }

    private void OnAllShinyClick(object? sender, RoutedEventArgs e) => ViewModel.GiveAllShinyBoostDonuts();
    private void OnAllLevel3Click(object? sender, RoutedEventArgs e) => ViewModel.GiveAllLevel3Donuts();
    private void OnRecalculateAllClick(object? sender, RoutedEventArgs e) => ViewModel.RecalculateAll();
    private void OnClearAllClick(object? sender, RoutedEventArgs e) => ViewModel.ClearAllDonuts();

    private void OnRecalculateClick(object? sender, RoutedEventArgs e) => ViewModel.RecalculateSelectedDonut();
    private void OnCloneCurrentClick(object? sender, RoutedEventArgs e) => ViewModel.CloneCurrentToAll();
    private void OnClearDonutClick(object? sender, RoutedEventArgs e) => ViewModel.ClearSelectedDonut();

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
