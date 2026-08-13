using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Service;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ServiceClientEntity = Spix.Domain.EntitiesGen.ServiceClient;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Service;

// Presenta categorias y sus servicios en el mismo indice, siguiendo el acordeon de Productos de Blazor.
public partial class ServiceIndexViewModel : PagedListViewModel<ServiceCategoryRowViewModel>
{
    private const int ChildPageSize = 100;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/servicecategories";

    public ServiceIndexViewModel(
        IPagedEntityService<ServiceCategoryRowViewModel> pagedEntityService,
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
    private async Task ToggleServicesAsync(ServiceCategoryRowViewModel? category)
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
        await LoadServicesAsync(category);
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateServiceCategoryDialogView>(
            "Crear categoria servicio");

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La categoria de servicios fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ServiceCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = category.ServiceCategoryId
        };

        var result = await _modalService.ShowAsync<EditServiceCategoryDialogView>(
            "Editar categoria servicio",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La categoria de servicios fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ServiceCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar categoria servicio",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync(
            $"api/v1/servicecategories/{category.ServiceCategoryId}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La categoria de servicios fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task NewServiceAsync(ServiceCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["ServiceCategoryId"] = category.ServiceCategoryId
        };

        var result = await _modalService.ShowAsync<CreateServiceClientDialogView>(
            "Crear servicio",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadExpandedCategoryAsync(category.ServiceCategoryId);
        await _alertService.SuccessAsync("Guardado", "El servicio fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditServiceAsync(ServiceClientEntity? service)
    {
        if (service is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = service.ServiceClientId
        };

        var result = await _modalService.ShowAsync<EditServiceClientDialogView>(
            "Editar servicio",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadExpandedCategoryAsync(service.ServiceCategoryId);
        await _alertService.SuccessAsync("Actualizado", "El servicio fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteServiceAsync(ServiceClientEntity? service)
    {
        if (service is null)
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

        var responseHttp = await _repository.DeleteAsync(
            $"api/v1/serviceclients/{service.ServiceClientId}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadExpandedCategoryAsync(service.ServiceCategoryId);
        await _alertService.SuccessAsync("Eliminado", "El servicio fue eliminado correctamente.");
    }

    // Obtiene los hijos cuando el usuario abre una categoria y evita descargas masivas del indice.
    private async Task LoadServicesAsync(ServiceCategoryRowViewModel category)
    {
        category.IsServicesLoading = true;
        category.ServicesMessage = string.Empty;

        try
        {
            var responseHttp = await _repository.GetAsync<List<ServiceClientEntity>>(
                $"api/v1/serviceclients?guidId={category.ServiceCategoryId}&page=1&recordsnumber={ChildPageSize}");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            category.Services = new ObservableCollection<ServiceClientEntity>(
                responseHttp.Response ?? new List<ServiceClientEntity>());

            if (category.Services.Count == 0)
            {
                category.ServicesMessage = "No hay servicios registrados en esta categoria.";
            }
        }
        catch (Exception exception)
        {
            category.ServicesMessage = exception.Message;
        }
        finally
        {
            category.IsServicesLoading = false;
        }
    }

    // Vuelve a cargar el padre y conserva expandida la categoria afectada por el CRUD hijo.
    private async Task ReloadExpandedCategoryAsync(Guid serviceCategoryId)
    {
        await LoadAsync(CurrentPage);

        var category = Items.FirstOrDefault(item => item.ServiceCategoryId == serviceCategoryId);
        if (category is null)
        {
            return;
        }

        category.IsExpanded = true;
        await LoadServicesAsync(category);
    }
}
