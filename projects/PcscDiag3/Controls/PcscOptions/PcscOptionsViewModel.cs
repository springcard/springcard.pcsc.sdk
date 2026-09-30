using PcscDiag3.Configurations;
using ReactiveUI;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC.Enums;

namespace PcscDiag3.Controls.PcscOptions;

/// <summary>
/// ViewModel for the PC/SC options dialog.
/// </summary>
public class PcscOptionsViewModel : WindowViewModel
{
    public bool IsUserScopeChecked { get; set; }
    public bool IsSystemScopeChecked { get; set; }
    public bool IsTerminalScopeChecked { get; set; }

    public bool IsAllFilteringChecked { get; set; }
    public bool IsDefaultFilteringChecked { get; set; }
    public bool IsSpecificFilteringChecked { get; set; }
    public string? SpecificFilteringValue { get; set; }

    private readonly IWritableOptions<ApplicationOptions> _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="PcscOptionsViewModel"/> class.
    /// </summary>
    public PcscOptionsViewModel(IWritableOptions<ApplicationOptions> configuration)
    {
        _configuration = configuration;

        SetScope();
        SetFiltering();
    }

    /// <summary>
    /// Cancels the dialog and closes the window without saving changes.
    /// </summary>
    public void CancelCommand() => Close(false);

    /// <summary>
    /// Confirms the dialog, updates configuration, and closes the window.
    /// </summary>
    public void OkCommand()
    {
        UpdateConfigurationScope();
        UpdateConfigurationFiltering();

        Close(true);
    }

    /// <summary>
    /// Sets the filtering selection based on the current configuration.
    /// </summary>
    private void SetFiltering()
    {
        switch (_configuration.Value.ListGroup)
        {
            case ReaderType.DEFAULT_READERS:
                IsDefaultFilteringChecked = true;
                this.RaisePropertyChanged(nameof(IsDefaultFilteringChecked));
                break;
            case ReaderType.ALL_READERS:
                IsAllFilteringChecked = true;
                this.RaisePropertyChanged(nameof(IsAllFilteringChecked));
                break;
            default:
                IsSpecificFilteringChecked = true;
                SpecificFilteringValue = _configuration.Value.ListGroup;
                this.RaisePropertyChanged(nameof(IsSpecificFilteringChecked));
                this.RaisePropertyChanged(nameof(SpecificFilteringValue));
                break;
        }
    }

    /// <summary>
    /// Sets the scope selection based on the current configuration.
    /// </summary>
    private void SetScope()
    {
        switch (_configuration.Value.ContextScope)
        {
            case ReaderScope.Terminal:
                IsTerminalScopeChecked = true;
                this.RaisePropertyChanged(nameof(IsTerminalScopeChecked));
                break;
            case ReaderScope.System:
                IsSystemScopeChecked = true;
                this.RaisePropertyChanged(nameof(IsSystemScopeChecked));
                break;
            case ReaderScope.User:
            default:
                IsUserScopeChecked = true;
                this.RaisePropertyChanged(nameof(IsUserScopeChecked));
                break;
        }
    }

    /// <summary>
    /// Updates the configuration with the selected reader filtering option.
    /// </summary>
    private void UpdateConfigurationFiltering()
    {
        if (IsAllFilteringChecked)
            _configuration.Update(x => x.ListGroup = ReaderType.ALL_READERS);
        else if (IsDefaultFilteringChecked)
            _configuration.Update(x => x.ListGroup = ReaderType.DEFAULT_READERS);
        else if (IsSpecificFilteringChecked && !string.IsNullOrEmpty(SpecificFilteringValue))
            _configuration.Update(x => x.ListGroup = SpecificFilteringValue);
    }

    /// <summary>
    /// Updates the configuration with the selected reader scope.
    /// </summary>
    private void UpdateConfigurationScope()
    {
        if (IsUserScopeChecked)
            _configuration.Update(x => x.ContextScope = ReaderScope.User);
        else if (IsSystemScopeChecked)
            _configuration.Update(x => x.ContextScope = ReaderScope.System);
        else if (IsTerminalScopeChecked)
            _configuration.Update(x => x.ContextScope = ReaderScope.Terminal);
    }
}
