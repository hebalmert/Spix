using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using PlanEntity = Spix.Domain.EntitiesGen.Plan;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Plan;

// Comparte la carga de impuestos y unidades de velocidad usada por los formularios de planes.
public abstract partial class PlanFormViewModel : CrudFormViewModel<PlanEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _taxes = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _speedUnits = new();

    protected override string BaseUrl
    {
        get
        {
            return "api/v1/plans";
        }
    }

    protected PlanFormViewModel(
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

    public int SelectedSpeedUpType
    {
        get
        {
            return (int)Entity.SpeedUpType;
        }
        set
        {
            Entity.SpeedUpType = (SpeedUpType)value;
            OnPropertyChanged();
        }
    }

    public int SelectedSpeedDownType
    {
        get
        {
            return (int)Entity.SpeedDownType;
        }
        set
        {
            Entity.SpeedDownType = (SpeedDownType)value;
            OnPropertyChanged();
        }
    }

    protected override PlanEntity CreateEntity()
    {
        return new PlanEntity
        {
            Active = true,
            SpeedUpType = SpeedUpType.M,
            SpeedDownType = SpeedDownType.M
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.PlanName))
        {
            return "Debes ingresar el nombre del plan.";
        }

        if (Entity.TasaReuso is null || Entity.TasaReuso <= 0)
        {
            return "Debes ingresar una tasa de reuso valida.";
        }

        if (Entity.Price <= 0)
        {
            return "Debes ingresar un precio de venta valido.";
        }

        return null;
    }

    // Mantiene los valores de los ComboBox alineados con las mismas consultas del formulario Blazor.
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var taxesResponse = await _repository.GetAsync<List<GuidItemModel>>(
                "api/v1/combosData/ComboTaxes");

            if (await _responseHandler.HandleErrorAsync(taxesResponse))
            {
                return;
            }

            Taxes = new ObservableCollection<GuidItemModel>(
                taxesResponse.Response ?? new List<GuidItemModel>());

            var speedResponse = await _repository.GetAsync<List<IntItemModel>>(
                "api/v1/combosData/ComboUp");

            if (await _responseHandler.HandleErrorAsync(speedResponse))
            {
                return;
            }

            SpeedUnits = new ObservableCollection<IntItemModel>(
                speedResponse.Response ?? new List<IntItemModel>());
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

    // Refresca las unidades visibles despues de cargar un plan existente.
    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
        OnPropertyChanged(nameof(SelectedSpeedUpType));
        OnPropertyChanged(nameof(SelectedSpeedDownType));
    }
}

public partial class CreatePlanDialogViewModel : PlanFormViewModel
{
    public CreatePlanDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    public void SetPlanCategory(Guid planCategoryId)
    {
        Entity.PlanCategoryId = planCategoryId;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditPlanDialogViewModel : PlanFormViewModel
{
    public EditPlanDialogViewModel(
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
