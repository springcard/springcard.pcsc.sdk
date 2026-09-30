using Avalonia.Controls;
using PcscDiag3.Controls.PcscOptions;

namespace PcscDiag3.Controls.CardCommunication.Disconnect;

public partial class DisconnectWindow : Window
{
    public DisconnectWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is DisconnectViewModel viewModel)
        {
            viewModel.CancelCommand();
        }
    }
}