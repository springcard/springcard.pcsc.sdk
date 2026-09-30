using Avalonia.Controls;
using PcscDiag3.Controls.PcscOptions;

namespace PcscDiag3.Controls.CardCommunication.Connect;

public partial class ConnectWindow : Window
{
    public ConnectWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is ConnectViewModel viewModel)
        {
            viewModel.CancelCommand();
        }
    }
}