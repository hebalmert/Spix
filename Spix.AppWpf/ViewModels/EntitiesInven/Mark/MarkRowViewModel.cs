using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkEntity = Spix.Domain.EntitiesGen.Mark;
using MarkModelEntity = Spix.Domain.EntitiesGen.MarkModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Mark;

// Mantiene el estado visual del acordeon sin modificar la entidad Marca.
public class MarkRowViewModel : MarkEntity, INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isModelsLoading;
    private ObservableCollection<MarkModelEntity> _models = new();
    private string _modelsMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsModelsLoading
    {
        get => _isModelsLoading;
        set => SetProperty(ref _isModelsLoading, value);
    }

    public ObservableCollection<MarkModelEntity> Models
    {
        get => _models;
        set
        {
            if (SetProperty(ref _models, value))
            {
                OnPropertyChanged(nameof(HasModels));
            }
        }
    }

    public string ModelsMessage
    {
        get => _modelsMessage;
        set
        {
            if (SetProperty(ref _modelsMessage, value))
            {
                OnPropertyChanged(nameof(HasModelsMessage));
            }
        }
    }

    public bool HasModels => Models.Count > 0;

    public bool HasModelsMessage => !string.IsNullOrWhiteSpace(ModelsMessage);

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
