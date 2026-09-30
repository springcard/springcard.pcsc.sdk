using Avalonia.Controls;
using System;

namespace PcscDiag3.Controls.CardCommunication.Informations;

public partial class InformationsView : UserControl
{
    public InformationsView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is InformationsViewModel viewModel)
            viewModel.SetEditors(CommandHexEditor, ResponseHexEditor);

        base.OnDataContextChanged(e);
    }
}