using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class LegalityReportWindow : Window
{
    public LegalityReportViewModel ViewModel => (LegalityReportViewModel)DataContext!;

    public LegalityReportWindow()
    {
        InitializeComponent();
    }

    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        if (Clipboard != null)
        {
            await Clipboard.SetTextAsync(ViewModel.FullReport);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
