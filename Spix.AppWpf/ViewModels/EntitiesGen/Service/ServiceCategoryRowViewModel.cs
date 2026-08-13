using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ServiceCategoryEntity = Spix.Domain.EntitiesGen.ServiceCategory;
using ServiceClientEntity = Spix.Domain.EntitiesGen.ServiceClient;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Service;

// Agrega el estado visual del acordeon sin alterar la entidad enviada por el Backend.
public class ServiceCategoryRowViewModel : ServiceCategoryEntity, INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isServicesLoading;
    private ObservableCollection<ServiceClientEntity> _services = new();
    private string _servicesMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsServicesLoading
    {
        get => _isServicesLoading;
        set => SetProperty(ref _isServicesLoading, value);
    }

    public ObservableCollection<ServiceClientEntity> Services
    {
        get => _services;
        set
        {
            if (!SetProperty(ref _services, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasServices));
        }
    }

    public string ServicesMessage
    {
        get => _servicesMessage;
        set
        {
            if (!SetProperty(ref _servicesMessage, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasServicesMessage));
        }
    }

    public bool HasServices => Services.Count > 0;

    public bool HasServicesMessage => !string.IsNullOrWhiteSpace(ServicesMessage);

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
