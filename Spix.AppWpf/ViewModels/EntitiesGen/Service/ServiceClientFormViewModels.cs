using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ServiceClientEntity = Spix.Domain.EntitiesGen.ServiceClient;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Service;

// Comparte la carga de impuestos usada por los formularios de servicios.
public abstract partial class ServiceClientFormViewModel : CrudFormViewModel<ServiceClientEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _taxes = new();

    protected override string BaseUrl
    {
        get
        {
            return "api/v1/serviceclients";
        }
    }

    protected ServiceClientFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    protected override ServiceClientEntity CreateEntity()
    {
        return new ServiceClientEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        return null;
    }

    // Mantiene el ComboBox de impuestos con la misma fuente que Blazor.
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var responseHttp = await _repository.GetAsync<List<GuidItemModel>>(
                "api/v1/combosData/ComboTaxes");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            Taxes = new ObservableCollection<GuidItemModel>(
                responseHttp.Response ?? new List<GuidItemModel>());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class CreateServiceClientDialogViewModel : ServiceClientFormViewModel
{
    public CreateServiceClientDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    public void SetServiceCategory(Guid serviceCategoryId)
    {
        Entity.ServiceCategoryId = serviceCategoryId;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditServiceClientDialogViewModel : ServiceClientFormViewModel
{
    public EditServiceClientDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
