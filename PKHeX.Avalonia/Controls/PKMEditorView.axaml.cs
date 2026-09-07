using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Controls;

public partial class PKMEditorView : UserControl
{
    public PKMEditorViewModel ViewModel => (PKMEditorViewModel)DataContext!;

    public event Action? LegalityReportRequested;
    public event Action? ExportPKMRequested;

    public PKMEditorView()
    {
        InitializeComponent();
    }

    public void OnShinyToggleClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ToggleShiny();
    }

    public void OnGenderToggleClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ToggleGender();
    }

    public void OnRerollPIDClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.RerollPID();
    }

    public void OnLegalityBadgeClick(object? sender, RoutedEventArgs e)
    {
        LegalityReportRequested?.Invoke();
    }

    public void OnExportPKMClick(object? sender, RoutedEventArgs e)
    {
        ExportPKMRequested?.Invoke();
    }
}
