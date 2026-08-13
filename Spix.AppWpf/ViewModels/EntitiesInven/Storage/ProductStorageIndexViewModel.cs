using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Storage;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Storage;

// Lista bodegas paginadas y conserva las acciones del indice Blazor.
public partial class ProductStorageIndexViewModel : PagedListViewModel<ProductStorage>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/productstorages";

    public ProductStorageIndexViewModel(IPagedEntityService<ProductStorage> pagedEntityService, IRepository repository, ModalService modalService, AlertService alertService, HttpResponseHandler responseHandler)
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
        var result = await _modalService.ShowAsync<CreateProductStorageDialogView>("Crear bodega");
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La bodega fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ProductStorage? storage)
    {
        if (storage is null) return;
        var result = await _modalService.ShowAsync<EditProductStorageDialogView>("Editar bodega", new Dictionary<string, object> { ["Id"] = storage.ProductStorageId });
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La bodega fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ProductStorage? storage)
    {
        if (storage is null) return;
        var confirmed = await _alertService.ConfirmAsync("Eliminar bodega", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed) return;
        var response = await _repository.DeleteAsync($"api/v1/productstorages/{storage.ProductStorageId}");
        if (await _responseHandler.HandleErrorAsync(response)) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La bodega fue eliminada correctamente.");
    }
}
