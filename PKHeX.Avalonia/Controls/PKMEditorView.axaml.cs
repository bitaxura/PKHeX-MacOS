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
    public event Action? ShowdownExportRequested;
    public event Action? ShowdownImportRequested;
    public event Action? RibbonsRequested;

    public PKMEditorView()
    {
        InitializeComponent();
    }

    public void OnShinyToggleClick(object? sender, RoutedEventArgs e) => ViewModel.ToggleShiny();
    public void OnGenderToggleClick(object? sender, RoutedEventArgs e) => ViewModel.ToggleGender();
    public void OnRerollPIDClick(object? sender, RoutedEventArgs e) => ViewModel.RerollPID();
    public void OnLegalityBadgeClick(object? sender, RoutedEventArgs e) => LegalityReportRequested?.Invoke();
    public void OnExportPKMClick(object? sender, RoutedEventArgs e) => ExportPKMRequested?.Invoke();
    public void OnShowdownExportClick(object? sender, RoutedEventArgs e) => ShowdownExportRequested?.Invoke();
    public void OnShowdownImportClick(object? sender, RoutedEventArgs e) => ShowdownImportRequested?.Invoke();
    public void OnOpenRibbonsClick(object? sender, RoutedEventArgs e) => RibbonsRequested?.Invoke();

    public void OnHyperTrainAllClick(object? sender, RoutedEventArgs e) => ViewModel.HyperTrainAll();
    public void OnHyperTrainClearClick(object? sender, RoutedEventArgs e) => ViewModel.HyperTrainClear();

    public void OnInfectPokerusClick(object? sender, RoutedEventArgs e) => ViewModel.InfectPokerus();
    public void OnCurePokerusClick(object? sender, RoutedEventArgs e) => ViewModel.CurePokerus();
    public void OnClearPokerusClick(object? sender, RoutedEventArgs e) => ViewModel.ClearPokerus();
}
