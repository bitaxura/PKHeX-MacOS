using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class RibbonEditorWindow : Window
{
    public RibbonEditorViewModel ViewModel => (RibbonEditorViewModel)DataContext!;

    public RibbonEditorWindow()
    {
        InitializeComponent();
    }

    private void OnGiveAllClick(object? sender, RoutedEventArgs e) => ViewModel.GiveAll();
    private void OnClearAllClick(object? sender, RoutedEventArgs e) => ViewModel.ClearAll();

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(false);
}

