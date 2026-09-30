using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DynamicData;
using PcscDiag3.Configurations;
using PcscDiag3.Controls.CardCommunication;
using PcscDiag3.Controls.PcscOptions;
using PcscDiag3.Controls.ReaderAtrAnalysis;
using PcscDiag3.Controls.ReaderInformations;
using PcscDiag3.Enums;
using PcscDiag3.IoC;
using ReactiveUI;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.LibCs.Windows.UI.Services.Windows;
using SpringCard.PCSC;
using SpringCard.PCSC.Enums;
using SpringCard.PCSC.ReaderHelpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace PcscDiag3.Controls.Main;

/// <summary>
/// ViewModel for the main window of the application. 
/// </summary>
public class MainWindowViewModel : ReactiveObject
{
    private const string DefaultGeneralInformations = "?";
    private const string DefaultReadersInformations = "No PC/SC reader";

    public Window? MainWindow { get; set; }
    public HierarchicalTreeDataGridSource<ReaderModel> ReadersSource { get; }
    public string GeneralInformations { get; set; } = DefaultGeneralInformations;
    public string ReadersInformations { get; set; } = DefaultReadersInformations;
    public bool IsSelectedReaderCardAtrPresent { get; set; }

    private ObservableCollection<ReaderModel> _readerGroups;
    private SCardReaderList? _sCardReaders = null;

    private readonly IWritableOptions<ApplicationOptions> _configuration;
    private readonly IWindowService _windowService;

    public MainWindowViewModel(IWritableOptions<ApplicationOptions> configuration, IWindowService windowService)
    {
        _configuration = configuration;
        _windowService = windowService;

        _readerGroups = new ObservableCollection<ReaderModel>();
        ReadersSource = new HierarchicalTreeDataGridSource<ReaderModel>(_readerGroups)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ReaderModel>(
                    new TemplateColumn<ReaderModel>(string.Empty, "ReaderIconCell", options: new() { CanUserResizeColumn = false, CanUserSortColumn = false }),
                    x => x.Readers,
                    isExpandedSelector: x => x.IsExpanded),
                new TextColumn<ReaderModel, string>("Reader Name", r => r.Name),
                new TemplateColumn<ReaderModel>(string.Empty, "ReaderStatusCell", options: new() { CanUserResizeColumn = false, CanUserSortColumn = false }),
                new TextColumn<ReaderModel, string>("Status", r => r.Status),
                new TextColumn<ReaderModel, string>("Card ATR", r => r.CardAtr)
            }
        };

        ReadersSource.RowSelection!.SelectionChanged += MainWindowViewModel_SelectionChanged;

        StartReaderMonitoring();
    }

    /// <summary>
    /// Shows the About dialog window.
    /// </summary>
    public void AboutCommand() => _windowService.ShowAboutDialogAsync(MainWindow);

    /// <summary>
    /// Opens the ATR analysis dialog for the selected reader if an ATR is present.
    /// </summary>
    public void AtrAnalysisCommand()
    {
        var selectedItem = ReadersSource.RowSelection?.SelectedItem;
        if (selectedItem == null || string.IsNullOrEmpty(selectedItem.CardAtr)) return;

        _windowService.ShowDialogAsync<ReaderAtrAnalysisWindow, ReaderAtrAnalysisViewModel, ReaderAtrAnalysisViewModelParam, bool>(MainWindow, new(selectedItem.CardAtr));
    }

    /// <summary>
    /// Placeholder for adding the ATR to the "no drivers" list.
    /// </summary>
    public void AddAtrToNoDriversCommand() { }

    /// <summary>
    /// Opens the direct control dialog for the selected reader.
    /// </summary>
    public void ControlDirectCommand()
    {
        var selectedItem = ReadersSource.RowSelection?.SelectedItem;
        if (selectedItem == null) return;

        _windowService.ShowWindowAsync<CardCommunicationWindow, CardCommunicationViewModel, CardCommunicationViewModelParam, string>
            (MainWindow, new(selectedItem.Name, selectedItem.ReaderIcon, ReaderShare.Direct, (ReaderProtocol)SCARD.PROTOCOL_DIRECT()));
    }

    /// <summary>
    /// Copies the ATR of the selected reader to the clipboard.
    /// </summary>
    public void CopyAtrToClipboardCommand()
    {
        var cardAtr = ReadersSource.RowSelection?.SelectedItem?.CardAtr;
        if (!string.IsNullOrEmpty(cardAtr))
            MainWindow?.Clipboard?.SetTextAsync(cardAtr);
    }

    /// <summary>
    /// Closes the main window and stops reader monitoring.
    /// </summary>
    public void ExitCommand()
    {
        StopReaderMonitoring();
        //MainWindow?.Close();
    }

    /// <summary>
    /// Opens the selected reader in shared or direct mode depending on ATR presence.
    /// </summary>
    public void OpenSelectedReader()
    {
        if (IsSelectedReaderCardAtrPresent) TransmitSharedCommand();
        else ControlDirectCommand();
    }

    /// <summary>
    /// Opens the PC/SC options dialog and restarts reader monitoring if options are changed.
    /// </summary>
    public async Task PcscOptionsCommand()
    {
        if (await _windowService.ShowDialogAsync<PcscOptionsWindow, PcscOptionsViewModel, bool>(MainWindow))
            StartReaderMonitoring();
    }

    /// <summary>
    /// Opens the reader information dialog for the selected reader.
    /// </summary>
    public void ReaderInfoCommand()
        => _windowService.ShowWindowAsync<ReaderInformationsWindow, ReaderInformationsViewModel, ReaderInformationsViewModelParam, bool>
            (MainWindow, new(ReadersSource.RowSelection?.SelectedItem?.Name ?? string.Empty));

    public void RestoreWindowPlacement()
    {
        var options = _configuration.Value;
        if (options.MainWindowLocation == null || MainWindow == null)
            return;

        var locationScreenBounds = options.MainWindowLocation.ScreenBounds;
        var lastScreenBounds = new PixelRect(locationScreenBounds.X, locationScreenBounds.Y, locationScreenBounds.Width, locationScreenBounds.Height);
        
        // Check if the screen still exists (user might have unplugged one)
        var screen = MainWindow.Screens.All
            .FirstOrDefault(s => s.Bounds == lastScreenBounds);

        if (screen != null)
            MainWindow.Position = new PixelPoint(options.MainWindowLocation.X, options.MainWindowLocation.Y);

        MainWindow.WindowState = options.MainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    public void SaveExitLocation()
    {
        if (MainWindow == null) return;
        var maximized = MainWindow.WindowState == WindowState.Maximized;
        var position = MainWindow.Position;
        var screen = MainWindow.Screens.ScreenFromVisual(MainWindow);

        if (screen == null) return;

        _configuration.Update(options =>
        {
            var screenBounds = new ScreenBounds(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
            options.MainWindowLocation = new MainWindowLocation(screenBounds, position.X, position.Y);
        });
    }

    /// <summary>
    /// Opens the card communication dialog in exclusive mode for the selected reader.
    /// </summary>
    public void TransmitExclusiveCommand()
    {
        var selectedItem = ReadersSource.RowSelection?.SelectedItem;
        if (selectedItem == null) return;

        _windowService.ShowWindowAsync<CardCommunicationWindow, CardCommunicationViewModel, CardCommunicationViewModelParam, string>
            (MainWindow, new(selectedItem.Name, selectedItem.ReaderIcon, ReaderShare.Exclusive, ReaderProtocol.T0 | ReaderProtocol.T1));
    }

    /// <summary>
    /// Opens the card communication dialog in shared mode for the selected reader.
    /// </summary>
    public void TransmitSharedCommand()
    {
        var selectedItem = ReadersSource.RowSelection?.SelectedItem;
        if (selectedItem == null) return;

        _windowService.ShowWindowAsync<CardCommunicationWindow, CardCommunicationViewModel, CardCommunicationViewModelParam, string>
            (MainWindow, new(selectedItem.Name, selectedItem.ReaderIcon, ReaderShare.Shared, ReaderProtocol.T0 | ReaderProtocol.T1));
    }

    /// <summary>
    /// Returns the slot order and icon image for a given slot name.
    /// </summary>
    private (int slotOrder, Bitmap? image) GetSlotInformations(string slotName)
        => slotName.ToLower() switch
        {
            "nfc" => (0, ReaderIcon.Contactless),
            "rfid" => (1, ReaderIcon.Contactless),
            "contactless" => (2, ReaderIcon.Contactless),
            "contact" => (3, ReaderIcon.Contact),
            "id-1" => (4, ReaderIcon.Contact),
            "sam" => (5, ReaderIcon.SimSam),
            "sim/sam" => (6, ReaderIcon.SimSam),
            "se" => (7, ReaderIcon.SimSam),
            _ => (10, null)
        };

    /// <summary>
    /// Handles selection changes in the readers tree grid.
    /// Ensures only readers (not groups) are selectable and updates ATR presence state.
    /// </summary>
    private void MainWindowViewModel_SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<ReaderModel> e)
    {
        IReadOnlyList<ReaderModel?> selectedItems = e.SelectedItems;
        if (selectedItems.FirstOrDefault() is ReaderModel readerModel)
        {
            // Cannot select a group
            if (readerModel.Readers.Count > 0)
                ReadersSource.RowSelection!.Deselect(e.SelectedIndexes.FirstOrDefault());

            IsSelectedReaderCardAtrPresent = !string.IsNullOrEmpty(ReadersSource.RowSelection?.SelectedItem?.CardAtr);
            this.RaisePropertyChanged(nameof(IsSelectedReaderCardAtrPresent));
        }
    }

    /// <summary>
    /// Callback invoked when the status of PC/SC readers changes.
    /// Updates the reader state or the reader list as needed.
    /// </summary>
    private void SCardReadersChanged(string name, uint state, CardBuffer cardAtr)
    {
        Task.Run(() => Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(name))
                UpdateReaderState(name, (ReaderState)state, cardAtr);
            else
                UpdateReaderList();
        }));
    }

    /// <summary>
    /// Starts monitoring the list of PC/SC readers and updates the UI accordingly.
    /// </summary>
    private void StartReaderMonitoring()
    {
        StopReaderMonitoring();

        var generalInformations = _configuration.Value.ContextScope switch
        {
            ReaderScope.User => "User scope",
            ReaderScope.System => "System scope",
            ReaderScope.Terminal => "Terminal scope",
            _ => $"Scope= {_configuration.Value.ContextScope}"
        };

        generalInformations += _configuration.Value.ListGroup switch
        {
            ReaderType.ALL_READERS => ", All readers",
            ReaderType.DEFAULT_READERS => ", Default readers",
            _ => $", {_configuration.Value.ListGroup}"
        };

        Task.Run(() => Dispatcher.UIThread.Post(() =>
        {
            GeneralInformations = generalInformations;
            this.RaisePropertyChanged(nameof(GeneralInformations));
        }));

        _sCardReaders = new SCardReaderList(_configuration.Value.ContextScope, _configuration.Value.ListGroup);
        //UpdateReaderList();
        _sCardReaders.StartMonitor(new SCardReaderList.StatusChangeCallback(SCardReadersChanged));
    }

    /// <summary>
    /// Stops monitoring the list of PC/SC readers.
    /// </summary>
    private void StopReaderMonitoring()
    {
        if (_sCardReaders != null)
        {
            _sCardReaders.StopMonitor();
            _sCardReaders = null;
        }
    }

    /// <summary>
    /// Updates the state and status of a specific reader.
    /// </summary>
    private void UpdateReaderState(string name, ReaderState state, CardBuffer cardAtr)
    {
        var reader = _readerGroups
            .SelectMany(x => x.Readers)
            .FirstOrDefault(reader => reader.Name == name);

        if (reader == null) return;

        switch (state)
        {
            case var s when s.HasFlag(ReaderState.Present | ReaderState.InUse | ReaderState.Exclusive):
                reader.Status = "In use (exclusive)";
                reader.StatusIcon = ReaderStatusIcon.CardExclusive;
                break;
            case var s when s.HasFlag(ReaderState.Present | ReaderState.InUse):
                reader.Status = "In use (shared)";
                reader.StatusIcon = ReaderStatusIcon.CardInuse;
                break;
            case var s when s.HasFlag(ReaderState.Present | ReaderState.Mute):
                reader.Status = "Mute";
                reader.StatusIcon = ReaderStatusIcon.CardMute;
                break;
            case var s when s.HasFlag(ReaderState.Present | ReaderState.Unpowered):
                reader.Status = "Present, not powered";
                reader.StatusIcon = ReaderStatusIcon.CardPresent;
                break;
            case var s when s.HasFlag(ReaderState.Present):
                reader.Status = "Present, powered";
                reader.StatusIcon = ReaderStatusIcon.CardPresent;
                break;
            case var s when s.HasFlag(ReaderState.Unavailable):
                reader.Status = "Reserved (direct)";
                reader.StatusIcon = ReaderStatusIcon.ReaderUnavailable;
                break;
            case var s when s.HasFlag(ReaderState.Ignore):
                reader.Status = "Error (ignore)";
                reader.StatusIcon = ReaderStatusIcon.Error;
                break;
            case var s when s.HasFlag(ReaderState.Unknown):
                reader.Status = "Error (status unknown)";
                reader.StatusIcon = ReaderStatusIcon.Unknown;
                break;
            case var s when s.HasFlag(ReaderState.Empty):
                reader.Status = "Absent";
                reader.StatusIcon = ReaderStatusIcon.CardAbsent;
                break;
            default:
                reader.Status = $"Bad status";
                reader.StatusIcon = ReaderStatusIcon.Error;
                break;
        }

        reader.CardAtr = cardAtr != null ? cardAtr.AsString("") : string.Empty;

        if (ReadersSource.RowSelection!.SelectedItem?.Name == name)
        {
            IsSelectedReaderCardAtrPresent = !string.IsNullOrEmpty(reader.CardAtr);
            this.RaisePropertyChanged(nameof(IsSelectedReaderCardAtrPresent));
        }
    }

    /// <summary>
    /// Updates the list of readers, adding new readers and removing those that are no longer present.
    /// </summary>
    private void UpdateReaderList()
    {
        if (_sCardReaders == null)
        {
            ReadersInformations = DefaultReadersInformations;
            this.RaisePropertyChanged(nameof(GeneralInformations));
            return;
        }

        var sCardReaders = _sCardReaders.Readers.ToList();
        if (sCardReaders.Count == 0)
        {
            ReadersInformations = DefaultReadersInformations;
            this.RaisePropertyChanged(nameof(ReadersInformations));
            _readerGroups.Clear();
            return;
        }

        ReadersInformations = $"{sCardReaders.Count} PC/SC reader(s)";
        this.RaisePropertyChanged(nameof(ReadersInformations));

        // Remove readers that are no longer present
        _readerGroups
            .SelectMany(x => x.Readers)
            .Where(r => !sCardReaders.Contains(r.Name))
            .ToList()
            .ForEach(x => _readerGroups.First(g => g.Readers.Any(r => r.Name == x.Name)).Readers.Remove(x));

        // Add new readers
        var readersToAdd = sCardReaders.Where(r => !_readerGroups.SelectMany(x => x.Readers).Any(x => x.Name == r)).ToList();

        var lastReaderKey = string.Empty;
        ReaderModel? lastGroup = null;

        foreach (var readerName in readersToAdd)
        {
            // Get reader informations
            ReaderInfos.ExplainReaderName(readerName, out var vendorName, out var productName, out var slotName, out var index);
            var isSpringCard = ReaderInfos.IsSpringCard(readerName);
            var (slotOrder, image) = GetSlotInformations(slotName);
            var key = $"{vendorName}|{productName}|{index}";
            var title = $"{productName} {index}";

            // Create group if previous reader is not the same device
            if (lastReaderKey != key)
            {
                lastReaderKey = key;

                if (isSpringCard)
                {
                    lastGroup = new ReaderModel { Name = title };
                    _readerGroups.Insert(0, lastGroup);
                }
                else
                {
                    lastGroup = new ReaderModel { Name= "Non-SpringCard readers" };
                    _readerGroups.Insert(_readerGroups.Count, lastGroup);
                }
            }

            // Add reader to the group
            lastGroup?.Readers.Add(new ReaderModel
            {
                ReaderIcon = image,
                Name = readerName
            });
        }

        // Remove empty groups
        _readerGroups.Where(x => x.Readers.Count == 0).ToList().ForEach(x => _readerGroups.Remove(x));
    }
}
