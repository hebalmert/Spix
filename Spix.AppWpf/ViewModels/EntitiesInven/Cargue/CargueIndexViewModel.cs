using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Cargue;

// Lista los cargues generados al cerrar compras de productos que manejan seriales.
//
// Lee el MISMO endpoint que la web (cargueboard) y no la tabla en crudo: ese le devuelve
// cada cargue con su avance ya contado por la base, que es lo que se pinta en la barra.
public partial class CargueIndexViewModel : PagedListViewModel<CargueListItemDto>
{
    private const string BoardUrl = "api/v1/cargueboard";

    private readonly IRepository _repository;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => BoardUrl;

    //Los cuatro numeros de arriba: son de toda la corporacion, no de la pagina
    [ObservableProperty]
    private CargueSummaryDto? _summary;

    public event EventHandler<Guid>? DetailsRequested;

    public CargueIndexViewModel(
        IPagedEntityService<CargueListItemDto> pagedEntityService,
        IRepository repository,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    // Despues de cada carga se vuelven a pedir los numeros: subir o borrar un cargue los cambia.
    protected override async Task AfterLoadAsync()
    {
        var responseHttp = await _repository.GetAsync<CargueSummaryDto>($"{BoardUrl}/summary");

        if (responseHttp.Error)
        {
            //El tablero es informativo: si no llega, el listado sigue funcionando
            return;
        }

        Summary = responseHttp.Response;
    }

    // Abre el detalle de cualquier cargue para revisar sus seriales y su estado.
    [RelayCommand]
    private void Details(CargueListItemDto? cargue)
    {
        if (cargue is null)
        {
            return;
        }

        DetailsRequested?.Invoke(this, cargue.CargueId);
    }

    // Conserva la regla de Blazor: solo un cargue pendiente puede eliminarse.
    [RelayCommand]
    private async Task DeleteAsync(CargueListItemDto? cargue)
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
