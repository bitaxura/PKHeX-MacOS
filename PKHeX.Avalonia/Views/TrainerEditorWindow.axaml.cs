using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class TrainerEditorWindow : Window
{
    public TrainerEditorViewModel ViewModel => (TrainerEditorViewModel)DataContext!;

    public TrainerEditorWindow()
    {
        InitializeComponent();
    }

    private void OnMaxCashClick(object? sender, RoutedEventArgs e) => ViewModel.SetMaxCash();
    private void OnMaxLPClick(object? sender, RoutedEventArgs e) => ViewModel.SetMaxLP();
    private void OnMaxBPClick(object? sender, RoutedEventArgs e) => ViewModel.SetMaxBP();
    private void OnMaxWattsClick(object? sender, RoutedEventArgs e) => ViewModel.SetMaxWatts();
    private void OnMaxMeritClick(object? sender, RoutedEventArgs e) => ViewModel.SetMaxMerit();

    private void OnSelectAllBadgesClick(object? sender, RoutedEventArgs e) => ViewModel.ToggleAllBadges(true);
    private void OnClearAllBadgesClick(object? sender, RoutedEventArgs e) => ViewModel.ToggleAllBadges(false);

    private void OnMaxRoyaleRegularClick(object? sender, RoutedEventArgs e) => ViewModel.MaxRoyaleRegular();
    private void OnMaxRoyaleInfiniteClick(object? sender, RoutedEventArgs e) => ViewModel.MaxRoyaleInfinite();
    private void OnMaxHyperspaceClick(object? sender, RoutedEventArgs e) => ViewModel.MaxHyperspace();
    private void OnCollectScrewsClick(object? sender, RoutedEventArgs e) => ViewModel.CollectScrews();
    private void OnUnlockTMsClick(object? sender, RoutedEventArgs e) => ViewModel.UnlockAllTMs();

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(false);
}
