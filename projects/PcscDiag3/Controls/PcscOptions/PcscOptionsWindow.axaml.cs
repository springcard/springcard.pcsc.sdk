using Avalonia.Controls;

namespace PcscDiag3.Controls.PcscOptions;

public partial class PcscOptionsWindow : Window
{
    public PcscOptionsWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is PcscOptionsViewModel viewModel)
        {
            viewModel.CancelCommand();
        }
    }
}