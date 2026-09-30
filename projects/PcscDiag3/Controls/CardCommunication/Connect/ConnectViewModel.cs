using PcscDiag3.Configurations;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.LibCs.Windows.UI;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC.Enums;

namespace PcscDiag3.Controls.CardCommunication.Connect;

public class ConnectResult
{
    public ReaderShare ShareMode { get; set; }
    public ReaderProtocol Protocol { get; set; }
}

public class ConnectViewModel : WindowViewModel
{
    public bool IsSharedAccessModeChecked { get; set; }
    public bool IsExclusiveAccessModeChecked { get; set; }
    public bool IsDirectAccessModeChecked { get; set; }

    public bool IsNoneProtocolChecked { get; set; }
    public bool IsT0ProtocolChecked { get; set; }
    public bool IsT1ProtocolChecked { get; set; }
    public bool IsT0T1ProtocolChecked { get; set; }
    public bool IsRawProtocolChecked { get; set; }

    private readonly IWritableOptions<ApplicationOptions> _configuration;

    public ConnectViewModel(IWritableOptions<ApplicationOptions> configuration)
    {
        _configuration = configuration;

        switch (_configuration.Value.ShareMode)
        {
            case ReaderShare.Shared: IsSharedAccessModeChecked = true; break;
            case ReaderShare.Direct: IsDirectAccessModeChecked = true; break;
            case ReaderShare.Exclusive:
            default: IsExclusiveAccessModeChecked = true; break;
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
        ReaderShare shareMode;
        if (IsSharedAccessModeChecked) shareMode = ReaderShare.Shared;
        if (IsDirectAccessModeChecked) shareMode = ReaderShare.Direct;
        else shareMode = ReaderShare.Exclusive;

        ReaderProtocol protocol;
        if (IsNoneProtocolChecked) protocol = ReaderProtocol.None;
        if (IsRawProtocolChecked) protocol = ReaderProtocol.Raw;
        if (IsT0ProtocolChecked) protocol = ReaderProtocol.T0;
        if (IsT1ProtocolChecked) protocol = ReaderProtocol.T1;
        else protocol = ReaderProtocol.T0 | ReaderProtocol.T1;

        _configuration.Update(x => x.ShareMode = shareMode);
        _configuration.Update(x => x.Protocol = protocol);

        Close(new ConnectResult
        {
            ShareMode = shareMode,
            Protocol = protocol
        });
    }

    public void CancelCommand()
    {
        Close(null);
    }
}
