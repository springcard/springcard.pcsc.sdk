using PcscDiag3.Configurations;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.LibCs.Windows.UI;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC.Enums;

namespace PcscDiag3.Controls.CardCommunication.Reconnect;

public class ReconnectResult
{
    public ReaderShare ShareMode { get; set; }
    public CardDisposing CardDisposing { get; set; }
    public ReaderProtocol Protocol { get; set; }
}

public class ReconnectViewModel : WindowViewModel
{
    public ReaderShare ShareMode { get; set; }

    public bool IsSharedAccessModeChecked { get; set; }
    public bool IsExclusiveAccessModeChecked { get; set; }
    public bool IsDirectAccessModeChecked { get; set; }

    public bool IsLeaveInitializationChecked { get; set; }
    public bool IsResetInitializationChecked { get; set; }
    public bool IsUnpowerInitializationChecked { get; set; }

    public bool IsNoneProtocolChecked { get; set; }
    public bool IsT0ProtocolChecked { get; set; }
    public bool IsT1ProtocolChecked { get; set; }
    public bool IsT0T1ProtocolChecked { get; set; }
    public bool IsRawProtocolChecked { get; set; }

    private readonly IWritableOptions<ApplicationOptions> _configuration;

    public ReconnectViewModel(IWritableOptions<ApplicationOptions> configuration)
    {
        _configuration = configuration;
        ShareMode = _configuration.Value.ShareMode;

        switch (_configuration.Value.ShareMode)
        {
            case ReaderShare.Shared: IsSharedAccessModeChecked = true; break;
            case ReaderShare.Direct: IsDirectAccessModeChecked = true; break;
            case ReaderShare.Exclusive:
            default: IsExclusiveAccessModeChecked = true; break;
        }

        switch (_configuration.Value.CardDisposing)
        {
            case CardDisposing.Leave: IsLeaveInitializationChecked = true; break;
            case CardDisposing.Unpower: IsUnpowerInitializationChecked = true; break;
            case CardDisposing.Reset:
            default: IsResetInitializationChecked = true; break;
        }

        switch (_configuration.Value.Protocol)
        {
            case ReaderProtocol.None: IsNoneProtocolChecked = true; break;
            case ReaderProtocol.Raw: IsRawProtocolChecked = true; break;
            case ReaderProtocol.T0: IsT0ProtocolChecked = true; break;
            case ReaderProtocol.T1: IsT1ProtocolChecked = true; break;
            case ReaderProtocol.T0 | ReaderProtocol.T1:
            default:
                IsT0T1ProtocolChecked = true; break;
        }
    }

    public void OkCommand()
    {
        var shareMode = ReaderShare.Exclusive;
        if (IsSharedAccessModeChecked) shareMode = ReaderShare.Shared;
        if (IsDirectAccessModeChecked) shareMode = ReaderShare.Direct;

        var cardDisposing = CardDisposing.Reset;
        if (IsLeaveInitializationChecked) cardDisposing = CardDisposing.Leave;
        if (IsUnpowerInitializationChecked) cardDisposing = CardDisposing.Unpower;

        var protocol = ReaderProtocol.T0 | ReaderProtocol.T1;
        if (IsNoneProtocolChecked) protocol = ReaderProtocol.None;
        if (IsRawProtocolChecked) protocol = ReaderProtocol.Raw;
        if (IsT0ProtocolChecked) protocol = ReaderProtocol.T0;
        if (IsT1ProtocolChecked) protocol = ReaderProtocol.T1;

        _configuration.Update(x => x.ShareMode = shareMode);
        _configuration.Update(x => x.CardDisposing = cardDisposing);
        _configuration.Update(x => x.Protocol = protocol);

        Close(new ReconnectResult
        {
            ShareMode = shareMode,
            CardDisposing = cardDisposing,
            Protocol = protocol
        });
    }

    public void CancelCommand()
    {
        Close(null);
    }
}
