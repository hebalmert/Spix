using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppMaui.Services;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppMaui.ViewModels;

//Cargar el servicio que se realizo. Las dos listas las arma el backend, igual que en la
//web: aqui no se filtra ni se ordena nada, y el precio sale del servicio elegido.
[QueryProperty(nameof(VisitId), "id")]
public partial class AddServiceViewModel : ObservableObject
{
    private const string BaseUrl = "api/v4/techvisits";
    private const string CategoryUrl = "/api/v1/combosData/ComboServiceCategories";
    private const string ServiceUrl = "/api/v1/combosData/ComboServiceClients";

    private readonly IRepository _repository;
    private readonly ApiResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string _visitId = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ServiceCategory> _categories = new();

    [ObservableProperty]
    private ObservableCollection<ServiceClient> _services = new();

    [ObservableProperty]
    private ServiceCategory? _selectedCategory;

    [ObservableProperty]
    private ServiceClient? _selectedService;

    [ObservableProperty]
    private string _detail = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public string PriceText => SelectedService is null
        ? string.Empty
        : SelectedService.Price.ToString("N2");

    public AddServiceViewModel(IRepository repository, ApiResponseHandler responseHandler, AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    partial void OnVisitIdChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        var response = await _repository.GetAsync<List<ServiceCategory>>(CategoryUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Categories = new ObservableCollection<ServiceCategory>(response.Response ?? new());
    }

    //Al elegir la categoria se bajan sus servicios: el backend decide cuales entran
    partial void OnSelectedCategoryChanged(ServiceCategory? value)
    {
        Services = new ObservableCollection<ServiceClient>();
        SelectedService = null;

        if (value is null || value.ServiceCategoryId == Guid.Empty)
        {
            return;
        }

        _ = CargarServiciosAsync(value.ServiceCategoryId);
    }

    partial void OnSelectedServiceChanged(ServiceClient? value) => OnPropertyChanged(nameof(PriceText));

    private async Task CargarServiciosAsync(Guid categoryId)
    {
        var response = await _repository.GetAsync<List<ServiceClient>>($"{ServiceUrl}/{categoryId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Services = new ObservableCollection<ServiceClient>(response.Response ?? new());
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (!Guid.TryParse(VisitId, out var visitId))
        {
            return;
        }

        if (SelectedService is null || SelectedService.ServiceClientId == Guid.Empty)
        {
            await _alertService.WarningAsync("Servicio", "Debes elegir el servicio que realizaste.");
            return;
        }

        IsBusy = true;

        try
        {
            //El impuesto y el total los calcula el backend: aqui no se hacen cuentas
            var dto = new ServiceRequestDetailDto
            {
                ServiceRequestId = visitId,
                ServiceCategoryId = SelectedService.ServiceCategoryId,
                ServiceClientId = SelectedService.ServiceClientId,
                Price = SelectedService.Price,
                Detail = string.IsNullOrWhiteSpace(Detail) ? null : Detail.Trim()
            };

            var response = await _repository.PostAsync<ServiceRequestDetailDto, ServiceRequestDetailDto>(
                $"{BaseUrl}/detail", dto);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }
        }
        finally
        {
            IsBusy = false;
        }

        await Shell.Current.GoToAsync("..");
    }
}
