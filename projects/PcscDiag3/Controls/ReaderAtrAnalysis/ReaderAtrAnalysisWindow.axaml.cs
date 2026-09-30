using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PcscDiag3.Controls.ReaderAtrAnalysis;

public partial class ReaderAtrAnalysisWindow : Window
{
    public ReaderAtrAnalysisWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}