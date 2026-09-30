using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Diagnostics;

namespace PcscDiag3.Controls.Main;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ReadersGrid_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2 && DataContext is MainWindowViewModel viewModel)
            viewModel.OpenSelectedReader();
    }

    private void SpringCardLogo_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://www.springcard.com/")
        {
            UseShellExecute = true,
            Verb = "open"
        });
    }

    private void TreeRowContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is ContextMenu contextMenu)
            contextMenu.DataContext = DataContext;
    }

    private void Window_Closed(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.ExitCommand();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.SaveExitLocation();
    }

    private void Window_Opened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.RestoreWindowPlacement();
    }
}