using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PcscDiag3.Controls.ReaderInformations;

public partial class ReaderInformationsWindow : Window
{
    public ReaderInformationsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}