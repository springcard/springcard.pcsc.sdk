using Avalonia.Input;
using AvaloniaHex;
using AvaloniaHex.Document;
using AvaloniaHex.Editing;
using PcscDiag3.Configurations;
using PcscDiag3.Enums;
using ReactiveUI;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PcscDiag3.Controls.CardCommunication.Informations;

/// <summary>
/// ViewModel for the card communication information panel.
/// </summary>
public class InformationsViewModel : ReactiveObject
{
    private CommunicationInformationsTab _tab;
    public CommunicationInformationsTab Tab
    {
        get => _tab;
        set
        {
            _tab = value;
            HandleTabChanged();
        }
    }

    public bool IsTransmitContentTextVisible { get; set; }
    public bool IsControlContentTextVisible { get; set; }
    public bool IsPreviousButtonEnabled => _currentHistory?.Previous != null;
    public bool IsNextButtonEnabled => _currentHistory?.Next != null;

    public IEnumerable<string>? History { get; set; }

    private int _selectedHistoryIndex = -1;
    public int SelectedHistoryIndex
    {
        get => _selectedHistoryIndex;
        set
        {
            var history = GetTabHistory();
            if (history == null) return;
            var item = GetElementAt(history, value);
            if (item == null) return;
            ClearCommand();
            SetCurrentHistory(item);
            SetCommandHexEditorContent(item.Value);
            _selectedHistoryIndex = -1;
        }
    }

    public string Response { get; set; } = string.Empty;
    public string ResponseMeaning { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorExplanation { get; set; } = string.Empty;

    public string Logs { get; set; } = string.Empty;

    public HexEditor? CommandHexEditor { get; set; }
    public HexEditor? ResponseHexEditor { get; set; }

    private readonly IWritableOptions<ApplicationOptions> _configuration;
    private Action _enterKeyPressed = () => { };

    private LinkedListNode<byte[]>? _currentHistory => Tab switch { CommunicationInformationsTab.Transmit => _currentTransmitHistory, CommunicationInformationsTab.Control => _currentControlHistory, _ => null };
    private LinkedListNode<byte[]>? _currentTransmitHistory;
    private LinkedListNode<byte[]>? _currentControlHistory;

#pragma warning disable CS8618
    /// <summary>
    /// Constructor used by Avalonia's previewer.
    /// </summary>
    public InformationsViewModel()
    {
        Tab = CommunicationInformationsTab.Transmit;
        _enterKeyPressed = () => { };
    }
#pragma warning restore CS8618

    /// <summary>
    /// Initializes a new instance of the <see cref="InformationsViewModel"/> class.
    /// </summary>
    public InformationsViewModel(IWritableOptions<ApplicationOptions> configuration) => _configuration = configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InformationsViewModel"/> class.
    /// Sets up history and key event handling.
    /// </summary>
    /// <param name="enterKeyPressed">Action to execute when Enter key is pressed.</param>
    public void Initialize(Action enterKeyPressed)
    {
        _enterKeyPressed = enterKeyPressed;

        _currentTransmitHistory = _configuration.Value.TransmitCommandHistory.Last;
        _currentControlHistory = _configuration.Value.ControlCommandHistory.Last;
    }

    /// <summary>
    /// Adds the current command and response to the history.
    /// Maintains a maximum of 10 history entries.
    /// </summary>
    public void AddHistory()
    {
        var history = GetTabHistory();
        if (history == null) return;

        history.AddLast(GetCommandValue());
        if (history.Count > 10)
            history.RemoveFirst();

        if (Tab == CommunicationInformationsTab.Transmit)
        {
            _currentTransmitHistory = history.Last;
            _configuration.Update(x => x.TransmitCommandHistory = history);
        }
        if (Tab == CommunicationInformationsTab.Control)
        {
            _currentControlHistory = history.Last;
            _configuration.Update(x => x.ControlCommandHistory = history);
        }

        History = history.Select(x => GetHistoryString(x, true));
        Logs += $"> {GetHistoryString(GetCommandValue(), false)}{Environment.NewLine}";
        Logs += $"< {GetHistoryString(GetResponseValue(), false)}{Environment.NewLine}";

        this.RaisePropertyChanged(nameof(History));
        this.RaisePropertyChanged(nameof(Logs));
        this.RaisePropertyChanged(nameof(IsPreviousButtonEnabled));
        this.RaisePropertyChanged(nameof(IsNextButtonEnabled));
    }

    /// <summary>
    /// Clears the command editor and response fields.
    /// </summary>
    public void ClearCommand()
    {
        if (CommandHexEditor?.Document != null)
            CommandHexEditor.Document.RemoveBytes(0, CommandHexEditor.Document.Length);
        ClearResponse();
    }

    /// <summary>
    /// Clears the response fields and updates the response editor.
    /// </summary>
    public void ClearResponse()
    {
        Response = string.Empty;
        ResponseMeaning = string.Empty;
        ErrorCode = string.Empty;
        ErrorExplanation = string.Empty;

        SetResponseHexEditorContent();

        this.RaisePropertyChanged(nameof(Response));
        this.RaisePropertyChanged(nameof(ResponseMeaning));
        this.RaisePropertyChanged(nameof(ErrorCode));
        this.RaisePropertyChanged(nameof(ErrorExplanation));
    }

    /// <summary>
    /// Gets the current command value from the command hex editor.
    /// </summary>
    /// <returns>Byte array of the command.</returns>
    public byte[] GetCommandValue()
        => CommandHexEditor?.Document is DynamicBinaryDocument document
            ? document.ToArray()
            : [];

    /// <summary>
    /// Navigates to the next command in history.
    /// </summary>
    public void NextCommand()
    {
        var next = _currentHistory?.Next;
        if (next != null)
        {
            SetCurrentHistory(next);
            ClearCommand();
            SetCommandHexEditorContent(next.Value);
        }
    }

    /// <summary>
    /// Navigates to the previous command in history.
    /// </summary>
    public void PreviousCommand()
    {
        var previous = _currentHistory?.Previous;
        if (previous != null)
        {
            SetCurrentHistory(previous);
            ClearCommand();
            SetCommandHexEditorContent(previous.Value);
        }
    }

    /// <summary>
    /// Sets the response value for control commands and updates the UI.
    /// </summary>
    /// <param name="value">Response byte array.</param>
    public void SetControlResponseValue(byte[] value)
    {
        if (ResponseHexEditor != null)
        {
            SetResponseHexEditorContent(value);

            if (value.Length > 0)
            {
                Response = $"{0 - value[0]}";

                if (value[0] == 0)
                    ResponseMeaning = "Success";
                else
                {
                    var sw = (ushort)(0x6F00 | value[0]);
                    ResponseMeaning = SCARD.CardStatusWordsToString(sw);
                }

                this.RaisePropertyChanged(nameof(Response));
                this.RaisePropertyChanged(nameof(ResponseMeaning));
            }
        }
    }

    /// <summary>
    /// Sets the command and response hex editors for the view.
    /// </summary>
    /// <param name="commandEditor">Hex editor for command input.</param>
    /// <param name="responseEditor">Hex editor for response output.</param>
    public void SetEditors(HexEditor commandEditor, HexEditor responseEditor)
    {
        CommandHexEditor = commandEditor;
        CommandHexEditor.HexView.AttachedToVisualTree += (_, _) => CommandHexEditor.HexView.Focus();
        CommandHexEditor.HexView.BytesPerLine = 16;
        CommandHexEditor.HexView.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) _enterKeyPressed();
        };
        CommandHexEditor.Caret.Mode = EditingMode.Insert;

        ResponseHexEditor = responseEditor;
        ResponseHexEditor.HexView.BytesPerLine = 16;
        ResponseHexEditor.Caret.Mode = EditingMode.Insert;

        SetCommandHexEditorContent(_currentHistory?.Value);
        SetResponseHexEditorContent();
    }

    /// <summary>
    /// Sets the error code and explanation in the UI.
    /// </summary>
    /// <param name="errorCode">Error code string.</param>
    /// <param name="errorMessage">Error explanation string.</param>
    public void SetError(string errorCode, string errorMessage)
    {
        ErrorCode = errorCode;
        ErrorExplanation = errorMessage;

        this.RaisePropertyChanged(nameof(ErrorCode));
        this.RaisePropertyChanged(nameof(ErrorExplanation));
    }

    /// <summary>
    /// Sets the response for transmit commands and updates the UI.
    /// </summary>
    /// <param name="response"><see cref="RAPDU"/> response object.</param>
    public void SetTransmitResponse(RAPDU response)
    {
        if (ResponseHexEditor != null)
        {
            SetResponseHexEditorContent(response.GetBytes());

            Response = $"{response.SW1:X2} {response.SW2:X2}";
            ResponseMeaning = SCARD.CardStatusWordsToString(response.SW);

            this.RaisePropertyChanged(nameof(Response));
            this.RaisePropertyChanged(nameof(ResponseMeaning));
        }
    }

    private LinkedListNode<byte[]>? GetElementAt(LinkedList<byte[]> history, int index)
        => GetElementAt(history.First, index, 0);

    private LinkedListNode<byte[]>? GetElementAt(LinkedListNode<byte[]>? node, int index, int currentIndex)
    {
        if (node == null) return null;
        if (index == currentIndex) return node;
        return GetElementAt(node.Next, index, currentIndex + 1);
    }

    private string GetHistoryString(byte[] value, bool truncate)
    {
        var result = BitConverter.ToString(value)
            .Replace('-', ' ');

        if (result.Length <= 47) return result;
        if (truncate) return result.Substring(0, 47) + " ..";

        // New line every 16 bytes
        // \S+  => At least one any non-whitespace character
        // \s   => Any whitespace character
        // ()   => Capturing group
        // (?:) => Non-capturing group
        // {15} => Exactly 15 repetitions of the precedent group or token
        // For more details : https://regex101.com/r/3Trv2x/1
        return Regex.Replace(result, @"(?:\S+\s){15}\S+(\s)", $"$0{Environment.NewLine}  ");
    }

    /// <summary>
    /// Gets the current response value from the response hex editor.
    /// </summary>
    /// <returns>Byte array of the response.</returns>
    private byte[] GetResponseValue()
        => ResponseHexEditor?.Document is DynamicBinaryDocument document
            ? document.ToArray()
            : [];

    private LinkedList<byte[]>? GetTabHistory()
        => Tab switch
        {
            CommunicationInformationsTab.Transmit => _configuration?.Value.TransmitCommandHistory,
            CommunicationInformationsTab.Control => _configuration?.Value.ControlCommandHistory,
            _ => null
        };

    /// <summary>
    /// Handles tab changes, updates visibility, resets command and response editors.
    /// </summary>
    private void HandleTabChanged()
    {
        IsTransmitContentTextVisible = Tab == CommunicationInformationsTab.Transmit;
        IsControlContentTextVisible = Tab == CommunicationInformationsTab.Control;

        this.RaisePropertyChanged(nameof(IsTransmitContentTextVisible));
        this.RaisePropertyChanged(nameof(IsControlContentTextVisible));

        CommandHexEditor?.HexView.Focus();

        ClearCommand();
        SetCommandHexEditorContent(_currentHistory?.Value);

        History = GetTabHistory()?.Select(x => GetHistoryString(x, true)) ?? [];
        this.RaisePropertyChanged(nameof(History));
    }

    /// <summary>
    /// Sets the content of the command hex editor.
    /// </summary>
    /// <param name="command">Command byte array.</param>
    private void SetCommandHexEditorContent(byte[]? command)
    {
        if (CommandHexEditor == null) return;

        CommandHexEditor!.Document = command != null
            ? new DynamicBinaryDocument(command)
            : new DynamicBinaryDocument();
    }

    /// <summary>
    /// Sets the content of the response hex editor.
    /// </summary>
    /// <param name="response">Response byte array (optional).</param>
    private void SetResponseHexEditorContent(byte[]? response = null)
    {
        if (ResponseHexEditor == null) return;

        ResponseHexEditor!.Document = response != null
            ? new DynamicBinaryDocument(response) { IsReadOnly = true }
            : new DynamicBinaryDocument() { IsReadOnly = true };
    }

    /// <summary>
    /// Sets the current history node for navigation.
    /// </summary>
    /// <param name="value">History node to set as current.</param>
    private void SetCurrentHistory(LinkedListNode<byte[]> value)
    {
        if (Tab == CommunicationInformationsTab.Transmit) _currentTransmitHistory = value;
        if (Tab == CommunicationInformationsTab.Control) _currentControlHistory = value;

        this.RaisePropertyChanged(nameof(IsPreviousButtonEnabled));
        this.RaisePropertyChanged(nameof(IsNextButtonEnabled));
    }
}
