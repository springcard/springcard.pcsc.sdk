using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData;
using ReactiveUI;
using SpringCard.LibCs.Windows.UI;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC;
using SpringCard.PCSC.Enums;
using SpringCard.PCSC.ReaderHelpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PcscDiag3.Controls.ReaderInformations;

public record ReaderInformationsViewModelParam(string ReaderName);

/// <summary>
/// ViewModel for the reader information dialog.
/// </summary>
public class ReaderInformationsViewModel : WindowWithParametersViewModel
{
    public string WindowTitle { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public FlatTreeDataGridSource<ReaderInformationModel> ReaderInformationsSource { get; }

    private ObservableCollection<ReaderInformationModel> _readerInformations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderInformationsViewModel"/> class.
    /// </summary>
    public ReaderInformationsViewModel()
    {
        _readerInformations = [];
        ReaderInformationsSource = new FlatTreeDataGridSource<ReaderInformationModel>(_readerInformations)
        {
            Columns =
            {
                new TextColumn<ReaderInformationModel, string>("Data", x => x.Title),
                new TextColumn<ReaderInformationModel, string>("Value", x => x.Value)
            }
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderInformationsViewModel"/> class.
    /// Connects to the specified reader and retrieves its information.
    /// </summary>
    /// <param name="readerName">The name of the reader to analyze.</param>
    public override void Initialize(object parameter)
    {
        if (parameter is not ReaderInformationsViewModelParam param)
            throw new ArgumentException($"Parameter must be of type ${nameof(ReaderInformationsViewModelParam)}.", nameof(parameter));

        WindowTitle = param.ReaderName;

        var channel = new SCardChannel(param.ReaderName);

        Device = OperatingSystem.IsWindows()
            ? SCARD.GetReaderDeviceInstanceId(channel.hContext, param.ReaderName).Replace("&", "&&")
            : param.ReaderName;

        channel.Protocol = SCARD.PROTOCOL_DIRECT();
        channel.ShareMode = (uint)ReaderShare.Direct;

        if (!channel.Connect())
        {
            // TODO: MessageBox
            //MessageBox.Show(this, "Failed to connect to the reader, information can't be retrieved.");
            return;
        }

        var readerHelper = new SpringCardReader(channel);

        if (!readerHelper.ReadData())
        {
            channel.DisconnectLeave();
            // TODO: MessageBox
            //MessageBox.Show(this, "Failed to read any information from the reader.");
            return;
        }

        channel.DisconnectLeave();

        _readerInformations.AddRange(readerHelper.Data.Select(x => new ReaderInformationModel(x.Key, x.Value)));

        this.RaisePropertyChanged(nameof(WindowTitle));
        this.RaisePropertyChanged(nameof(Device));
    }

    /// <summary>
    /// Copies all reader information to the clipboard in a formatted text representation.
    /// </summary>
    public void CopyToClipboardCommand()
    {
        var sb = new StringBuilder();
        foreach (var info in _readerInformations)
        {
            sb.AppendLine($"{info.Title}: {info.Value}");
        }

        CurrentWindow?.Clipboard?.SetTextAsync(sb.ToString());
    }
}
