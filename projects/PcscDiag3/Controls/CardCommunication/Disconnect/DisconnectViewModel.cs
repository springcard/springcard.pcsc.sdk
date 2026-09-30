using PcscDiag3.Configurations;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.LibCs.Windows.UI;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC.Enums;

namespace PcscDiag3.Controls.CardCommunication.Disconnect;

public class DisconnectResult
{
    public CardDisposing CardDisposing { get; set; }
}

public class DisconnectViewModel : WindowViewModel
{
    public bool IsLeaveInitializationChecked { get; set; }
    public bool IsResetInitializationChecked { get; set; }
    public bool IsUnpowerInitializationChecked { get; set; }
    public bool IsEjectInitializationChecked { get; set; }

    private readonly IWritableOptions<ApplicationOptions> _configuration;

    public DisconnectViewModel(IWritableOptions<ApplicationOptions> configuration)
    {
        _configuration = configuration;

        switch (_configuration.Value.CardDisposing)
        {
            case CardDisposing.Leave: IsLeaveInitializationChecked = true; break;
            case CardDisposing.Unpower: IsUnpowerInitializationChecked = true; break;
            case CardDisposing.Eject: IsEjectInitializationChecked = true; break;
            case CardDisposing.Reset:
            default: IsResetInitializationChecked = true; break;
        }
    }

    public void OkCommand()
    {
        var cardDisposing = CardDisposing.Reset;
        if (IsLeaveInitializationChecked) cardDisposing = CardDisposing.Leave;
        if (IsUnpowerInitializationChecked) cardDisposing = CardDisposing.Unpower;
        if (IsEjectInitializationChecked) cardDisposing = CardDisposing.Eject;

        _configuration.Update(x => x.CardDisposing = cardDisposing);

        Close(new DisconnectResult
        {
            CardDisposing = cardDisposing,
        });
    }

    public void CancelCommand()
    {
        Close();
    }
}
