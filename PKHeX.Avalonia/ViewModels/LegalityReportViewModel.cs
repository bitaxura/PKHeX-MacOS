using System;
using System.Collections.ObjectModel;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class LegalityCheckItemViewModel : ReactiveObject
{
    public Severity Severity { get; }
    public string Category { get; }
    public string Message { get; }

    public string SeverityIcon => Severity switch
    {
        Severity.Valid => "✓",
        Severity.Invalid => "✕",
        Severity.Fishy => "⚠",
        _ => "ℹ"
    };

    public string SeverityColor => Severity switch
    {
        Severity.Valid => "#2E7D32",
        Severity.Invalid => "#D32F2F",
        Severity.Fishy => "#F57C00",
        _ => "#1976D2"
    };

    public LegalityCheckItemViewModel(Severity severity, string category, string message)
    {
        Severity = severity;
        Category = category;
        Message = message;
    }
}

public sealed class LegalityReportViewModel : ReactiveObject
{
    public PKM Pokemon { get; }
    public LegalityAnalysis Analysis { get; }

    public bool IsValid => Analysis.Valid;
    public string HeaderText => IsValid ? "✓ Legal Pokémon" : "✕ Legality Issues Detected";
    public string HeaderColor => IsValid ? "#2E7D32" : "#D32F2F";

    public string EncounterSummary => Analysis.EncounterMatch?.Name ?? "Unknown Encounter";

    public string FullReport { get; }

    public ObservableCollection<LegalityCheckItemViewModel> Checks { get; } = [];

    public LegalityReportViewModel(PKM pkm)
    {
        Pokemon = pkm;
        Analysis = new LegalityAnalysis(pkm);

        var ctx = LegalityLocalizationContext.Create(Analysis, "en");
        FullReport = ctx.Report(true);

        foreach (var res in Analysis.Results)
        {
            if (res.IsNotGeneric() || res.Judgement != Severity.Valid)
            {
                string msg = res.Result.ToString();
                Checks.Add(new LegalityCheckItemViewModel(res.Judgement, res.Identifier.ToString(), msg));
            }
        }

        if (Checks.Count == 0 && IsValid)
        {
            Checks.Add(new LegalityCheckItemViewModel(Severity.Valid, "General", "All checks passed successfully."));
        }
    }
}
