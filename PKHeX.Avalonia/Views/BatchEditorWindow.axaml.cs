using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class BatchEditorWindow : Window
{
    public BatchEditorViewModel ViewModel => (BatchEditorViewModel)DataContext!;

    public BatchEditorWindow()
    {
        InitializeComponent();
    }

    private void OnRunClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Execute();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(ViewModel.ModifiedCount > 0);
    }
}
