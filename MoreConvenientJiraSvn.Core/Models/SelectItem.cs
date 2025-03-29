using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Models;

public record SelectItem : INotifyPropertyChanged
{
    private bool _isSelect;
    public bool IsSelected
    {
        get => _isSelect;
        set
        {
            if (_isSelect != value)
            {
                _isSelect = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    public string Name { get; set; } = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return Name;
    }
}
