using Avalonia.Controls;
using PcscDiag3.Controls.PcscOptions;

namespace PcscDiag3.Controls.CardCommunication.Reconnect;

public partial class ReconnectWindow : Window
{
    public ReconnectWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is ReconnectViewModel viewModel)
        {
            viewModel.CancelCommand();
        }
    }
}