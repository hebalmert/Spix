using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Plan;
using Spix.HttpService;
using PlanCategoryEntity = Spix.Domain.EntitiesGen.PlanCategory;
using PlanEntity = Spix.Domain.EntitiesGen.Plan;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Plan;

// Planes: maestro-detalle, igual que en la web. Todo lo que comparte con los otros
// tres modulos del catalogo vive en CategoryDetailViewModel; aqui solo lo propio.
public partial class PlanIndexViewModel : CategoryDetailViewModel<PlanCategoryEntity, PlanEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/plancategories";

    protected override string ChildEndpoint => "api/v1/plans";

    public PlanIndexViewModel(
        IPagedEntityService<PlanCategoryEntity> pagedEntityService,
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

    protected override Guid GetCategoryId(PlanCategoryEntity category)
    {
        return category.PlanCategoryId;
    }

    public override string SelectedCategoryName => SelectedCategory?.PlanCategoryName ?? "Planes";

    // Los dos indicadores propios de este modulo
    public int ActiveCount => Children.Count(item => item.Active);

    public int InactiveCount => Children.Count(item => !item.Active);

    protected override void NotifyKpis()
    {
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(InactiveCount));
    }

    protected override IEnumerable<PlanEntity> ApplyChip(IEnumerable<PlanEntity> children, string chip)
    {
        return chip switch
        {
            "active" => children.Where(item => item.Active),
            "inactive" => children.Where(item => !item.Active),
            _ => children
        };
    }

    protected override async Task<IReadOnlyCollection<PlanEntity>> GetChildrenAsync(string url)
    {
        var responseHttp = await _repository.GetAsync<List<PlanEntity>>(url);

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return Array.Empty<PlanEntity>();
        }

        return responseHttp.Response ?? new List<PlanEntity>();
    }

    // ===== La categoria =====

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreatePlanCategoryDialogView>("Crear categoria de planes");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La categoria de planes fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(PlanCategoryEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditPlanCategoryDialogView>(
            "Editar categoria de planes",
            new Dictionary<string, object> { ["Id"] = category.PlanCategoryId });

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La categoria de planes fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(PlanCategoryEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar categoria de planes",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{Endpoint}/{category.PlanCategoryId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La categoria de planes fue eliminada correctamente.");
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

        var result = await _modalService.ShowAsync<CreatePlanDialogView>(
            "Crear plan",
            new Dictionary<string, object> { ["PlanCategoryId"] = categoryId });

        if (!result.Succeeded)
        {
            return;
        }

        //Se recarga tambien el padre porque cambia el contador de la fila
        await ReloadKeepingSelectionAsync(categoryId);
        await _alertService.SuccessAsync("Guardado", "El plan fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditChildAsync(PlanEntity? child)
    {
        if (child is null || SelectedCategory is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditPlanDialogView>(
            "Editar plan",
            new Dictionary<string, object> { ["Id"] = child.PlanId });

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Actualizado", "El plan fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteChildAsync(PlanEntity? child)
    {
        if (child is null || SelectedCategory is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar plan",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{ChildEndpoint}/{child.PlanId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Eliminado", "El plan fue eliminado correctamente.");
    }
}
