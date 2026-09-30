using Avalonia.Controls;
using SpringCard.PCSC.Enums;

namespace PcscDiag3.Controls.CardCommunication;

public partial class CardCommunicationWindow : Window
{
    public CardCommunicationWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is CardCommunicationViewModel viewModel)
        {
            viewModel.Disconnect(CardDisposing.Reset);
            viewModel.Close(viewModel.WindowTitle);
        }
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is CardCommunicationViewModel viewModel)
            viewModel.Close(viewModel.WindowTitle);
        else Close();
    }
}