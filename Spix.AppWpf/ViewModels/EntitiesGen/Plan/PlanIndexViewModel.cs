using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Plan;
using Spix.HttpService;
using System.Collections.ObjectModel;
using PlanEntity = Spix.Domain.EntitiesGen.Plan;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Plan;

// Presenta categorias y sus planes en el mismo indice, siguiendo el acordeon de Productos de Blazor.
public partial class PlanIndexViewModel : PagedListViewModel<PlanCategoryRowViewModel>
{
    private const int ChildPageSize = 100;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/plancategories";

    public PlanIndexViewModel(
        IPagedEntityService<PlanCategoryRowViewModel> pagedEntityService,
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

    // Mantiene una sola categoria abierta para que el listado sea facil de recorrer.
    [RelayCommand]
    private async Task TogglePlansAsync(PlanCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        if (category.IsExpanded)
        {
            category.IsExpanded = false;
            return;
        }

        foreach (var item in Items)
        {
            item.IsExpanded = false;
        }

        category.IsExpanded = true;
        await LoadPlansAsync(category);
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreatePlanCategoryDialogView>(
            "Crear categoria plan");

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La categoria de planes fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(PlanCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = category.PlanCategoryId
        };

        var result = await _modalService.ShowAsync<EditPlanCategoryDialogView>(
            "Editar categoria plan",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La categoria de planes fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(PlanCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar categoria plan",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync(
            $"api/v1/plancategories/{category.PlanCategoryId}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La categoria de planes fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task NewPlanAsync(PlanCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["PlanCategoryId"] = category.PlanCategoryId
        };

        var result = await _modalService.ShowAsync<CreatePlanDialogView>(
            "Crear plan",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadExpandedCategoryAsync(category.PlanCategoryId);
        await _alertService.SuccessAsync("Guardado", "El plan fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditPlanAsync(PlanEntity? plan)
    {
        if (plan is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = plan.PlanId
        };

        var result = await _modalService.ShowAsync<EditPlanDialogView>(
            "Editar plan",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadExpandedCategoryAsync(plan.PlanCategoryId);
        await _alertService.SuccessAsync("Actualizado", "El plan fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeletePlanAsync(PlanEntity? plan)
    {
        if (plan is null)
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

        var responseHttp = await _repository.DeleteAsync($"api/v1/plans/{plan.PlanId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadExpandedCategoryAsync(plan.PlanCategoryId);
        await _alertService.SuccessAsync("Eliminado", "El plan fue eliminado correctamente.");
    }

    // Obtiene los hijos cuando el usuario abre una categoria y evita descargas masivas del indice.
    private async Task LoadPlansAsync(PlanCategoryRowViewModel category)
    {
        category.IsPlansLoading = true;
        category.PlansMessage = string.Empty;

        try
        {
            var responseHttp = await _repository.GetAsync<List<PlanEntity>>(
                $"api/v1/plans?guidId={category.PlanCategoryId}&page=1&recordsnumber={ChildPageSize}");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            category.Plans = new ObservableCollection<PlanEntity>(
                responseHttp.Response ?? new List<PlanEntity>());

            if (category.Plans.Count == 0)
            {
                category.PlansMessage = "No hay planes registrados en esta categoria.";
            }
        }
        catch (Exception exception)
        {
            category.PlansMessage = exception.Message;
        }
        finally
        {
            category.IsPlansLoading = false;
        }
    }

    // Vuelve a cargar el padre y conserva expandida la categoria afectada por el CRUD hijo.
    private async Task ReloadExpandedCategoryAsync(Guid planCategoryId)
    {
        await LoadAsync(CurrentPage);

        var category = Items.FirstOrDefault(item => item.PlanCategoryId == planCategoryId);
        if (category is null)
        {
            return;
        }

        category.IsExpanded = true;
        await LoadPlansAsync(category);
    }
}
