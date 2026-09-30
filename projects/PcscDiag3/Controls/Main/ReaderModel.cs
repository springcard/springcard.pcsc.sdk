using Avalonia.Media.Imaging;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PcscDiag3.Controls.Main;

public class ReaderModel : INotifyPropertyChanged
{
    public Bitmap? ReaderIcon { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsExpanded { get; set; } = true;
    public ObservableCollection<ReaderModel> Readers { get; set; } = [];

    public Bitmap? StatusIcon
    {
        get => _statusIcon;
        set
        {
            _statusIcon = value;
            OnPropertyChanged(nameof(StatusIcon));
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public string CardAtr
    {
        get => _cardAtr;
        set
        {
            _cardAtr = value;
            OnPropertyChanged(nameof(CardAtr));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private string _cardAtr = string.Empty;
    private Bitmap? _statusIcon;
    private string _status = string.Empty;

    protected virtual void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
