using ReactiveUI;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC.CardAnalysis;
using System;

namespace PcscDiag3.Controls.ReaderAtrAnalysis;

public record ReaderAtrAnalysisViewModelParam(string CardAtr);

/// <summary>
/// ViewModel for the ATR analysis dialog.
/// </summary>
public class ReaderAtrAnalysisViewModel : WindowWithParametersViewModel
{
    public string WindowTitle { get; set; } = string.Empty;
    public string ATR { get; set; } = string.Empty;
    public string TechnicalAnalysis { get; set; } = string.Empty;
    public string RecognizedCards { get; set; } = string.Empty;
    public string TecnicalInformations { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderAtrAnalysisViewModel"/> class.
    /// Performs ATR analysis and populates properties for display.
    /// </summary>
    /// <param name="readerName">The name of the reader (not used in analysis).</param>
    /// <param name="cardAtr">The ATR string to analyze.</param>
    public override void Initialize(object parameters)
    {
        if (parameters is not ReaderAtrAnalysisViewModelParam param)
            throw new ArgumentException($"Parameter must be of type {nameof(ReaderAtrAnalysisViewModelParam)}", nameof(parameters));

        WindowTitle = $"Card {param.CardAtr}";
        ATR = param.CardAtr;
        TechnicalAnalysis = string.Join('\n', new CardAtrParser(param.CardAtr).TextDescription());
        RecognizedCards = string.Join('\n', CardAtrList.Descriptions(param.CardAtr) ?? []);
        TecnicalInformations = string.Join('\n', CardPixList.Descriptions(param.CardAtr) ?? []);

        this.RaisePropertyChanged(nameof(WindowTitle));
        this.RaisePropertyChanged(nameof(ATR));
        this.RaisePropertyChanged(nameof(TechnicalAnalysis));
        this.RaisePropertyChanged(nameof(RecognizedCards));
        this.RaisePropertyChanged(nameof(TecnicalInformations));
    }
}
