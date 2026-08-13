using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Supplier;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using SupplierEntity = Spix.Domain.EntitiesInven.Supplier;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Supplier;

// Lista proveedores paginados y concentra sus acciones CRUD.
public partial class SupplierIndexViewModel : PagedListViewModel<SupplierEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/suppliers";

    public SupplierIndexViewModel(IPagedEntityService<SupplierEntity> pagedEntityService, IRepository repository, ModalService modalService, AlertService alertService, HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateSupplierDialogView>("Crear proveedor");
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El proveedor fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(SupplierEntity? supplier)
    {
        if (supplier is null) return;
        var result = await _modalService.ShowAsync<EditSupplierDialogView>("Editar proveedor", new Dictionary<string, object> { ["Id"] = supplier.SupplierId });
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El proveedor fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(SupplierEntity? supplier)
    {
        if (supplier is null) return;
        var confirmed = await _alertService.ConfirmAsync("Eliminar proveedor", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed) return;
        var response = await _repository.DeleteAsync($"api/v1/suppliers/{supplier.SupplierId}");
        if (await _responseHandler.HandleErrorAsync(response)) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El proveedor fue eliminado correctamente.");
    }
}
