using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;

// Carrusel de las fotos de la visita. Solo muestra: no sube ni borra.
public partial class ServicePhotoViewerDialogViewModel : ObservableObject
{
    private readonly ModalService _modalService;

    private List<ServiceRequestPhotoDto> _photos = new();
    private int _index;

    [ObservableProperty]
    private ServiceRequestPhotoDto? _current;

    public bool HasMany => _photos.Count > 1;

    public string Tag => Current?.PhotoType == ServicePhotoType.After ? "Despues" : "Antes";

    public string Meta => Current is null
        ? string.Empty
        : $"{Current.DateCreated.ToLocalTime():dd/MM/yyyy HH:mm} · {Current.UserByName}";

    public ServicePhotoViewerDialogViewModel(ModalService modalService)
    {
        _modalService = modalService;
    }

    // Se abre en la foto que toco el usuario, no en la primera
    public void Initialize(List<ServiceRequestPhotoDto> fotos, Guid inicial)
    {
        _photos = fotos;

        var posicion = _photos.FindIndex(x => x.ServiceRequestPhotoId == inicial);
        _index = posicion < 0 ? 0 : posicion;

        Mostrar();
    }

    // Las flechas dan la vuelta: de la ultima se pasa a la primera
    [RelayCommand]
    private void Previous()
    {
        if (_photos.Count == 0)
        {
            return;
        }

        _index = _index == 0 ? _photos.Count - 1 : _index - 1;

        Mostrar();
    }

    [RelayCommand]
    private void Next()
    {
        if (_photos.Count == 0)
        {
            return;
        }

        _index = _index == _photos.Count - 1 ? 0 : _index + 1;

        Mostrar();
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private void Mostrar()
    {
        Current = _photos.Count == 0 ? null : _photos[_index];

        OnPropertyChanged(nameof(HasMany));
        OnPropertyChanged(nameof(Tag));
        OnPropertyChanged(nameof(Meta));
    }
}
