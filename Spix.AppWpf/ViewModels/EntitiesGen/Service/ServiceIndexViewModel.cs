using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Service;
using Spix.HttpService;
using ServiceCategoryEntity = Spix.Domain.EntitiesGen.ServiceCategory;
using ServiceClientEntity = Spix.Domain.EntitiesGen.ServiceClient;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Service;

// Servicios: maestro-detalle, igual que en la web. Todo lo que comparte con los otros
// tres modulos del catalogo vive en CategoryDetailViewModel; aqui solo lo propio.
public partial class ServiceIndexViewModel : CategoryDetailViewModel<ServiceCategoryEntity, ServiceClientEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/servicecategories";

    protected override string ChildEndpoint => "api/v1/serviceclients";

    public ServiceIndexViewModel(
        IPagedEntityService<ServiceCategoryEntity> pagedEntityService,
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

    protected override Guid GetCategoryId(ServiceCategoryEntity category)
    {
        return category.ServiceCategoryId;
    }

    public override string SelectedCategoryName => SelectedCategory?.Name ?? "Servicios";

    // Los dos indicadores propios de este modulo
    public int ActiveCount => Children.Count(item => item.Active);

    public int InactiveCount => Children.Count(item => !item.Active);

    protected override void NotifyKpis()
    {
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(InactiveCount));
    }

    protected override IEnumerable<ServiceClientEntity> ApplyChip(IEnumerable<ServiceClientEntity> children, string chip)
    {
        return chip switch
        {
            "active" => children.Where(item => item.Active),
            "inactive" => children.Where(item => !item.Active),
            _ => children
        };
    }

    protected override async Task<IReadOnlyCollection<ServiceClientEntity>> GetChildrenAsync(string url)
    {
        var responseHttp = await _repository.GetAsync<List<ServiceClientEntity>>(url);

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return Array.Empty<ServiceClientEntity>();
        }

        return responseHttp.Response ?? new List<ServiceClientEntity>();
    }

    // ===== La categoria =====

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateServiceCategoryDialogView>("Crear categoria de servicios");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La categoria de servicios fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ServiceCategoryEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditServiceCategoryDialogView>(
            "Editar categoria de servicios",
            new Dictionary<string, object> { ["Id"] = category.ServiceCategoryId });

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La categoria de servicios fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ServiceCategoryEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar categoria de servicios",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{Endpoint}/{category.ServiceCategoryId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La categoria de servicios fue eliminada correctamente.");
    }

    // ===== El hijo de la categoria =====

    [RelayCommand]
    private async Task NewChildAsync()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        var categoryId = GetCategoryId(SelectedCategory);

        var result = await _modalService.ShowAsync<CreateServiceClientDialogView>(
            "Crear servicio",
            new Dictionary<string, object> { ["ServiceCategoryId"] = categoryId });

        if (!result.Succeeded)
        {
            return;
        }

        //Se recarga tambien el padre porque cambia el contador de la fila
        await ReloadKeepingSelectionAsync(categoryId);
        await _alertService.SuccessAsync("Guardado", "El servicio fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditChildAsync(ServiceClientEntity? child)
    {
        if (child is null || SelectedCategory is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditServiceClientDialogView>(
            "Editar servicio",
            new Dictionary<string, object> { ["Id"] = child.ServiceClientId });

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Actualizado", "El servicio fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteChildAsync(ServiceClientEntity? child)
    {
        if (child is null || SelectedCategory is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar servicio",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{ChildEndpoint}/{child.ServiceClientId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Eliminado", "El servicio fue eliminado correctamente.");
    }
}
