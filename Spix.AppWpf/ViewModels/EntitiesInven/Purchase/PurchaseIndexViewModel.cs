using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Purchase;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Purchase;

// Mantiene el encabezado de compras y abre su detalle operativo sin cargar todas las lineas.
public partial class PurchaseIndexViewModel : PagedListViewModel<Spix.Domain.EntitiesInven.Purchase>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/purchases";

    public event EventHandler<Guid>? DetailsRequested;

    public PurchaseIndexViewModel(
        IPagedEntityService<Spix.Domain.EntitiesInven.Purchase> pagedEntityService,
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

    // El detalle controla sus productos y cierre, igual que DetailsPurchases de Blazor.
    [RelayCommand]
    private void Details(Spix.Domain.EntitiesInven.Purchase? purchase)
    {
        if (purchase is null)
        {
            return;
        }

        DetailsRequested?.Invoke(this, purchase.PurchaseId);
    }

    // Crea el encabezado pendiente antes de registrar las lineas de productos.
    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreatePurchaseDialogView>("Crear compra");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La compra fue guardada correctamente.");
    }

    // Permite corregir el encabezado de la compra con el mismo formulario de Blazor.
    [RelayCommand]
    private async Task EditAsync(Spix.Domain.EntitiesInven.Purchase? purchase)
    {
        if (purchase is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = purchase.PurchaseId
        };

        var result = await _modalService.ShowAsync<EditPurchaseDialogView>(
            "Editar compra",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La compra fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(Spix.Domain.EntitiesInven.Purchase? purchase)
    {
        if (purchase is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar compra",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"api/v1/purchases/{purchase.PurchaseId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La compra fue eliminada correctamente.");
    }
}
