using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Transfer;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using TransferEntity = Spix.Domain.EntitiesInven.Transfer;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Transfer;

// Los traslados de inventario: mueven stock de una bodega a otra.
//
// Replica de IndexTransfer de la web, con el mismo molde que Compras en el escritorio:
// el encabezado se mantiene aqui y las lineas viven en su propia pantalla, para no
// descargar un traslado entero cada vez que se abre la lista.
//
// Va por v2 porque es la API del escritorio.
public partial class TransferIndexViewModel : PagedListViewModel<TransferEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v2/transfers";

    public event EventHandler<Guid>? DetailsRequested;

    public TransferIndexViewModel(
        IPagedEntityService<TransferEntity> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    //Un traslado cerrado ya movio el inventario: no se edita ni se borra
    private static bool EstaPendiente(TransferEntity? transfer)
    {
        return transfer?.Status == TransferType.Pendiente;
    }

    [RelayCommand]
    private void Details(TransferEntity? transfer)
    {
        if (transfer is null)
        {
            return;
        }

        DetailsRequested?.Invoke(this, transfer.TransferId);
    }

    //El historial se arma con la fila que ya esta en pantalla: no hace falta ir al servidor
    [RelayCommand]
    private async Task AuditAsync(TransferEntity? transfer)
    {
        if (transfer is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Transfer"] = transfer
        };

        await _modalService.ShowAsync<AuditTransferDialogView>("Historial del traslado", parameters);
    }

    // El encabezado se crea pendiente; las lineas se agregan despues en el detalle
    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateTransferDialogView>("Crear traslado");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El traslado fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(TransferEntity? transfer)
    {
        if (!EstaPendiente(transfer))
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = transfer!.TransferId
        };

        var result = await _modalService.ShowAsync<EditTransferDialogView>("Editar traslado", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El traslado fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(TransferEntity? transfer)
    {
        if (!EstaPendiente(transfer))
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar traslado",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{transfer!.TransferId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El traslado fue eliminado correctamente.");
    }
}
