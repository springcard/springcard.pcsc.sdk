using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using PcscDiag3.Configurations;
using PcscDiag3.Controls.CardCommunication.Connect;
using PcscDiag3.Controls.CardCommunication.Disconnect;
using PcscDiag3.Controls.CardCommunication.Informations;
using PcscDiag3.Controls.CardCommunication.Reconnect;
using PcscDiag3.Enums;
using PcscDiag3.IoC;
using ReactiveUI;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.LibCs.Windows.UI;
using SpringCard.LibCs.Windows.UI.IoC.Factories;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.LibCs.Windows.Usb;
using SpringCard.PCSC;
using SpringCard.PCSC.Enums;
using System;
using System.Threading.Tasks;

namespace PcscDiag3.Controls.CardCommunication;

public record CardCommunicationViewModelParam(string Reader, Bitmap? ReaderIcon, ReaderShare Share, ReaderProtocol Protocol);

/// <summary>
/// ViewModel for the card communication dialog.
/// </summary>
public class CardCommunicationViewModel(
    IWritableOptions<ApplicationOptions> configuration, 
    IViewModelFactory viewModelFactory,
    IWindowService windowService) : WindowWithParametersViewModel
{
    private static readonly int DefaultWindowWidth = 800;
    private static readonly int DefaultLogsWidth = 350;

    public string WindowTitle { get; set; } = string.Empty;
    public WindowIcon? WindowIcon { get; set; }

    private CommunicationInformationsTab _selectedTabIndex;
    public CommunicationInformationsTab SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            _selectedTabIndex = value;
            InformationsViewModel.Tab = value;
            UpdateButtons();
        }
    }

    public bool IsControlButtonVisible { get; set; }
    public bool IsTransmitButtonVisible { get; set; }
    public bool IsConnectButtonEnabled { get; set; }
    public bool IsDisconnectButtonEnabled { get; set; }
    public bool IsReconnectButtonEnabled { get; set; }
    public bool IsTransmitButtonEnabled { get; set; }
    public bool IsControlButtonEnabled { get; set; }
    public bool IsLogsVisible { get; set; } = false;

    public int WindowWidth { get; set; } = DefaultWindowWidth;
    public int LogsWidth { get; set; } = 0;

    public InformationsViewModel InformationsViewModel { get; set; } = null!;

    public string CardAtr { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;

    private SCardChannel? _channel;
    private string _reader = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardCommunicationViewModel"/> class.
    /// Sets up the dialog and connects to the reader.
    /// </summary>
    public override void Initialize(object parameters)
    {
        if (parameters is not CardCommunicationViewModelParam param)
            throw new ArgumentException($"Parameter must be of type {nameof(CardCommunicationViewModelParam)}.", nameof(parameters));

        WindowTitle = param.Reader;

        if (param.ReaderIcon != null)
            WindowIcon = new WindowIcon(param.ReaderIcon);

        _reader = param.Reader;

        InformationsViewModel = viewModelFactory.Create<InformationsViewModel>();
        InformationsViewModel.Initialize(EnterKeyPressed);

        Connect(param.Share, param.Protocol);
    }

    /// <summary>
    /// Opens the connect dialog and connects to the reader with selected options.
    /// </summary>
    public async Task ConnectCommand()
    {
        var connectResult = await windowService.ShowDialogAsync<ConnectWindow, ConnectViewModel, ConnectResult>(
            CurrentWindow, new ConnectViewModel(configuration));

        if (connectResult != null)
            Connect(connectResult.ShareMode, connectResult.Protocol);
    }

    /// <summary>
    /// Sends a control command to the reader.
    /// </summary>
    public void ControlCommand()
        => SendCommand(commandValue => _channel?.Control(new CardBuffer(commandValue)));

    public void CopyLogToClipboardCommand()
        => CurrentWindow?.Clipboard!.SetTextAsync(InformationsViewModel.Logs);

    /// <summary>
    /// Disconnects from the reader and disposes the card according to the specified mode.
    /// </summary>
    /// <param name="disposing">Card disposing mode.</param>
    public void Disconnect(CardDisposing disposing)
    {
        CardAtr = string.Empty;
        Protocol = string.Empty;
        Mode = string.Empty;

        if (_channel != null && !_channel.Disconnect((uint)disposing))
            ShowError();
        else
            ShowSuccess();

        _channel = null;
    }

    /// <summary>
    /// Opens the disconnect dialog and disconnects from the reader with selected options.
    /// </summary>
    public async Task DisconnectCommand()
    {
        var disconnectResult = await windowService.ShowDialogAsync<DisconnectWindow, DisconnectViewModel, DisconnectResult>(
            CurrentWindow, new DisconnectViewModel(configuration));

        if (disconnectResult != null)
            Disconnect(disconnectResult.CardDisposing);
    }

    /// <summary>
    /// Opens the reconnect dialog and reconnects to the reader with selected options.
    /// </summary>
    public async Task ReconnectCommand()
    {
        if (_channel == null || !_channel.Connected)
            return;

        var reconnectResult = await windowService.ShowDialogAsync<ReconnectWindow, ReconnectViewModel, ReconnectResult>(
            CurrentWindow, new ReconnectViewModel(configuration));

        if (reconnectResult != null)
        {
            _channel.ShareMode = (uint)reconnectResult.ShareMode;
            _channel.Protocol = (uint)reconnectResult.Protocol;

            if (!_channel.Reconnect((uint)reconnectResult.CardDisposing))
            {
                ShowError();
                return;
            }
            ShowSuccess();

            CardAtr = _channel.CardAtr.ToString();
            Protocol = _channel.ProtocolAsString;
            Mode = _channel.ShareModeAsString;
        }
    }

    public void ShowLogs()
    {
        IsLogsVisible = !IsLogsVisible;
        WindowWidth = IsLogsVisible ? DefaultWindowWidth + LogsWidth : DefaultWindowWidth;
        LogsWidth = IsLogsVisible ? DefaultLogsWidth : 0;

        this.RaisePropertyChanged(nameof(IsLogsVisible));
        this.RaisePropertyChanged(nameof(WindowWidth));
        this.RaisePropertyChanged(nameof(LogsWidth));
    }

    /// <summary>
    /// Sends a transmit command (APDU) to the card.
    /// </summary>
    public void TransmitCommand()
        => SendCommand(commandValue => _channel?.Transmit(new CAPDU(commandValue)));

    /// <summary>
    /// Connects to the reader with the specified share mode and protocol.
    /// Updates the UI and state accordingly.
    /// </summary>
    /// <param name="share">Share mode.</param>
    /// <param name="protocol">Protocol.</param>
    private void Connect(ReaderShare share, ReaderProtocol protocol)
    {
        if (_channel != null)
            Disconnect(CardDisposing.Reset);

        _channel = new SCardChannel(_reader);
        _channel.ShareMode = (uint)share;
        _channel.Protocol = (uint)protocol;

        if (share == ReaderShare.Direct)
            SelectedTabIndex = CommunicationInformationsTab.Control;
        else
            SelectedTabIndex = CommunicationInformationsTab.Transmit;

        if (!_channel.Connect())
        {
            ShowError();
            return;
        }

        ShowSuccess();

        CardAtr = _channel.CardAtr.ToString();
        Protocol = _channel.ProtocolAsString;
        Mode = _channel.ShareModeAsString;
    }

    /// <summary>
    /// Handles the Enter key pressed event, sending the appropriate command based on the selected tab.
    /// </summary>
    private void EnterKeyPressed()
    {
        if (_selectedTabIndex == CommunicationInformationsTab.Transmit) TransmitCommand();
        else ControlCommand();
    }

    /// <summary>
    /// Sends a command to the reader and updates the response in the UI.
    /// </summary>
    /// <param name="sendCommand">Function to send the command and get the response.</param>
    private void SendCommand(Func<byte[], object?> sendCommand)
    {
        InformationsViewModel.ClearResponse();
        var commandValue = InformationsViewModel.GetCommandValue();

        var response = sendCommand(commandValue);
        if (response == null)
            ShowError();
        else
        {
            ShowSuccess();

            switch (response)
            {
                case RAPDU rapdu:
                    InformationsViewModel.SetTransmitResponse(rapdu);
                    break;
                case CardBuffer cb:
                    InformationsViewModel.SetControlResponseValue(cb.GetBytes());
                    break;
            }

            InformationsViewModel.AddHistory();
        }
    }

    /// <summary>
    /// Displays an error message in the UI based on the last error from the channel.
    /// </summary>
    private void ShowError()
    {
        var error = _channel?.LastError;
        var errorExplain = _channel?.LastErrorAsString ?? string.Empty;
        var errorCode = (error & 0x80000000) != 0
            ? $"{error:X8}"
            : $"{error}";

        InformationsViewModel.SetError(errorCode, errorExplain);
        UpdateButtons();
    }

    /// <summary>
    /// Clears any error message in the UI and updates button states.
    /// </summary>
    private void ShowSuccess()
    {
        InformationsViewModel.SetError(string.Empty, string.Empty);
        UpdateButtons();
    }

    /// <summary>
    /// Updates the visibility and enabled state of all dialog buttons based on the current connection and protocol state.
    /// </summary>
    private void UpdateButtons()
    {
        var readerConnected = _channel != null && _channel.Connected;
        var protocol = (ReaderProtocol?)_channel?.Protocol;
        var cardConnected = readerConnected && (protocol == ReaderProtocol.T0 || protocol == ReaderProtocol.T1);

        IsControlButtonVisible = SelectedTabIndex == CommunicationInformationsTab.Control;
        IsTransmitButtonVisible = SelectedTabIndex == CommunicationInformationsTab.Transmit;

        IsConnectButtonEnabled = !readerConnected;
        IsDisconnectButtonEnabled = readerConnected;
        IsReconnectButtonEnabled = readerConnected && cardConnected;
        IsTransmitButtonEnabled = IsTransmitButtonVisible && readerConnected && cardConnected;
        IsControlButtonEnabled = IsControlButtonVisible && readerConnected;

        this.RaisePropertyChanged(nameof(IsControlButtonVisible));
        this.RaisePropertyChanged(nameof(IsTransmitButtonVisible));

        this.RaisePropertyChanged(nameof(IsConnectButtonEnabled));
        this.RaisePropertyChanged(nameof(IsDisconnectButtonEnabled));
        this.RaisePropertyChanged(nameof(IsReconnectButtonEnabled));
        this.RaisePropertyChanged(nameof(IsTransmitButtonEnabled));
        this.RaisePropertyChanged(nameof(IsControlButtonEnabled));
    }
}
