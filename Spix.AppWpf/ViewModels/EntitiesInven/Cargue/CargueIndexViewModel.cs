using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Cargue;

// Lista los cargues generados al cerrar compras de productos que manejan seriales.
public partial class CargueIndexViewModel : PagedListViewModel<Spix.Domain.EntitiesInven.Cargue>
{
    private readonly IRepository _repository;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/cargues";

    public event EventHandler<Guid>? DetailsRequested;

    public CargueIndexViewModel(
        IPagedEntityService<Spix.Domain.EntitiesInven.Cargue> pagedEntityService,
        IRepository repository,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    // Abre el detalle de cualquier cargue para revisar sus seriales y su estado.
    [RelayCommand]
    private void Details(Spix.Domain.EntitiesInven.Cargue? cargue)
    {
        if (cargue is null)
        {
            return;
        }

        DetailsRequested?.Invoke(this, cargue.CargueId);
    }

    // Abre la carga de MAC cuando aun hay seriales pendientes por registrar.
    [RelayCommand]
    private void UploadSerials(Spix.Domain.EntitiesInven.Cargue? cargue)
    {
        if (cargue is null)
        {
            return;
        }

        DetailsRequested?.Invoke(this, cargue.CargueId);
    }

    // Conserva la regla de Blazor: solo un cargue pendiente puede eliminarse.
    [RelayCommand]
    private async Task DeleteAsync(Spix.Domain.EntitiesInven.Cargue? cargue)
    {
        if (cargue is null || cargue.Status != CargueType.Pendiente)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar cargue",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"api/v1/cargues/{cargue.CargueId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El cargue fue eliminado correctamente.");
    }
}
