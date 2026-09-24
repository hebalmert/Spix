using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Mark;
using Spix.HttpService;
using MarkEntity = Spix.Domain.EntitiesGen.Mark;
using MarkModelEntity = Spix.Domain.EntitiesGen.MarkModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Mark;

// Marcas: maestro-detalle, igual que en la web. Todo lo que comparte con los otros
// tres modulos del catalogo vive en CategoryDetailViewModel; aqui solo lo propio.
public partial class MarkIndexViewModel : CategoryDetailViewModel<MarkEntity, MarkModelEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/marks";

    protected override string ChildEndpoint => "api/v1/marksmodels";

    public MarkIndexViewModel(
        IPagedEntityService<MarkEntity> pagedEntityService,
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

    protected override Guid GetCategoryId(MarkEntity category)
    {
        return category.MarkId;
    }

    public override string SelectedCategoryName => SelectedCategory?.MarkName ?? "Modelos";

    // Los dos indicadores propios de este modulo
    public int ActiveCount => Children.Count(item => item.Active);

    public int InactiveCount => Children.Count(item => !item.Active);

    protected override void NotifyKpis()
    {
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(InactiveCount));
    }

    protected override IEnumerable<MarkModelEntity> ApplyChip(IEnumerable<MarkModelEntity> children, string chip)
    {
        return chip switch
        {
            "active" => children.Where(item => item.Active),
            "inactive" => children.Where(item => !item.Active),
            _ => children
        };
    }

    protected override async Task<IReadOnlyCollection<MarkModelEntity>> GetChildrenAsync(string url)
    {
        var responseHttp = await _repository.GetAsync<List<MarkModelEntity>>(url);

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return Array.Empty<MarkModelEntity>();
        }

        return responseHttp.Response ?? new List<MarkModelEntity>();
    }

    // ===== La categoria =====

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateMarkDialogView>("Crear marca");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La marca fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(MarkEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditMarkDialogView>(
            "Editar marca",
            new Dictionary<string, object> { ["Id"] = category.MarkId });

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La marca fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(MarkEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar marca",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{Endpoint}/{category.MarkId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La marca fue eliminada correctamente.");
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

        var result = await _modalService.ShowAsync<CreateMarkModelDialogView>(
            "Crear modelo",
            new Dictionary<string, object> { ["MarkId"] = categoryId });

        if (!result.Succeeded)
        {
            return;
        }

        //Se recarga tambien el padre porque cambia el contador de la fila
        await ReloadKeepingSelectionAsync(categoryId);
        await _alertService.SuccessAsync("Guardado", "El modelo fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditChildAsync(MarkModelEntity? child)
    {
        if (child is null || SelectedCategory is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditMarkModelDialogView>(
            "Editar modelo",
            new Dictionary<string, object> { ["Id"] = child.MarkModelId });

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Actualizado", "El modelo fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteChildAsync(MarkModelEntity? child)
    {
        if (child is null || SelectedCategory is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar modelo",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{ChildEndpoint}/{child.MarkModelId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Eliminado", "El modelo fue eliminado correctamente.");
    }
}
