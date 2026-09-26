using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ClientEntity = Spix.Domain.EntitiesOper.Client;
using ContractClientEntity = Spix.Domain.EntitiesContratos.ContractClient;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;

// El formulario del contrato, replicado del FormContractClient de la web.
//
// Dos cosas que no son obvias y vienen de la web:
//  - Al elegir el CLIENTE se copian su telefono y su direccion: el contrato nace con los
//    datos del cliente y desde ahi se corrigen si hace falta.
//  - El correo NO se edita: es el del cliente, y es a donde llega la solicitud de firma.
//  - El ESTADO solo se toca mientras el contrato se esta armando. Ya operativo (Activo,
//    Exonerado, Suspendido, Anulado, Retirado) se muestra pero se cambia desde Control de
//    contratos, que es donde estan las reglas de ese cambio.
public abstract partial class ContractClientFormViewModel : ObservableObject
{
    protected const string BaseUrl = "api/v1/contractclients";

    private const string ClientsUrl = "api/v1/clients";
    private const string StatusUrl = "/api/v1/contractclients/loadContractClientStatus";
    private const string ContractorsUrl = "/api/v1/combosData/ComboContractor";
    private const string ClientsComboUrl = "/api/v1/combosData/ComboClients";
    private const string StatesUrl = "/api/v1/combosData/ComboState";
    private const string CitiesUrl = "/api/v1/combosData/ComboCity";
    private const string ZonesUrl = "/api/v1/zones/loadCombo";
    private const string StratumUrl = "/api/v1/estratossociales/loadCombo";

    //Desde dos letras: con una sola la busqueda traeria medio archivo
    private const int MinimoParaBuscar = 2;

    protected readonly IRepository _repository;
    protected readonly HttpResponseHandler _responseHandler;
    protected readonly ModalService _modalService;
    protected readonly AlertService _alertService;
    private readonly LanguageService _languageService;

    [ObservableProperty]
    private ContractClientEntity _entity = new();

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _contractors = new();

    [ObservableProperty]
    private ObservableCollection<State> _states = new();

    [ObservableProperty]
    private ObservableCollection<City> _cities = new();

    [ObservableProperty]
    private ObservableCollection<Zone> _zones = new();

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _stratums = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _statuses = new();

    //Lo que va cayendo mientras se escribe el cliente
    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _clients = new();

    [ObservableProperty]
    private string _clientFilter = string.Empty;

    [ObservableProperty]
    private string? _clientEmail;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    protected bool IsEdit;

    //Mientras se cargan los combos en cascada, los eventos no deben limpiar lo cargado
    private bool _cargando;

    // El estado solo se edita mientras el contrato se esta armando
    public bool StatusEditable =>
        !IsEdit ||
        Entity.ContractState == ContractState.Draft ||
        Entity.ContractState == ContractState.PendingApproval ||
        Entity.ContractState == ContractState.InProgress;

    public bool StatusReadOnly => !StatusEditable;

    public string StatusText => _languageService.Text($"ContractState_{Entity.ContractState}");

    // El combo del estado trabaja con int, la entidad con el enum
    public int StatusValue
    {
        get => (int)Entity.ContractState;
        set
        {
            if ((int)Entity.ContractState == value)
            {
                return;
            }

            Entity.ContractState = (ContractState)value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }

    protected ContractClientFormViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        LanguageService languageService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _languageService = languageService;
    }

    protected async Task LoadCombosAsync()
    {
        await LoadContractorsAsync();
        await LoadStatusesAsync();
        await LoadStratumsAsync();
        await LoadStatesAsync();
    }

    //===================== El cliente =====================

    // Se busca desde dos letras; con menos se limpia lo que hubiera
    [RelayCommand]
    private async Task SearchClientsAsync(string? texto)
    {
        ClientFilter = texto ?? string.Empty;

        if (ClientFilter.Trim().Length < MinimoParaBuscar)
        {
            Clients = new ObservableCollection<GuidItemModel>();
            return;
        }

        var response = await _repository.GetAsync<List<GuidItemModel>>(
            $"{ClientsComboUrl}?filter={Uri.EscapeDataString(ClientFilter.Trim())}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Clients = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    // Al elegir el cliente, el contrato nace con SUS datos: telefono y direccion
    [RelayCommand]
    private async Task SelectClientAsync(GuidItemModel? cliente)
    {
        if (cliente is null)
        {
            return;
        }

        Entity.ClientId = cliente.Value;
        ClientFilter = cliente.Name ?? string.Empty;
        Clients = new ObservableCollection<GuidItemModel>();

        var response = await _repository.GetAsync<ClientEntity>($"{ClientsUrl}/{cliente.Value}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response;
        if (datos is null)
        {
            return;
        }

        Entity.PhoneNumber = datos.PhoneNumber;
        Entity.Address = datos.Address;

        //El correo no se edita: es el del cliente, y es a donde llega la solicitud de firma
        ClientEmail = datos.Email;

        OnPropertyChanged(nameof(Entity));
    }

    //===================== Estado, ciudad y zona =====================

    partial void OnEntityChanged(ContractClientEntity value)
    {
        OnPropertyChanged(nameof(StatusEditable));
        OnPropertyChanged(nameof(StatusReadOnly));
        OnPropertyChanged(nameof(StatusValue));
        OnPropertyChanged(nameof(StatusText));
    }

    // Al cambiar de estado se limpian ciudad y zona: las de antes ya no valen
    public async Task ChangeStateAsync(int stateId)
    {
        if (_cargando)
        {
            return;
        }

        Entity.StateId = stateId;
        Entity.CityId = 0;
        Entity.ZoneId = Guid.Empty;

        Cities = new ObservableCollection<City>();
        Zones = new ObservableCollection<Zone>();

        await LoadCitiesAsync(stateId);
    }

    public async Task ChangeCityAsync(int cityId)
    {
        if (_cargando)
        {
            return;
        }

        Entity.CityId = cityId;
        Entity.ZoneId = Guid.Empty;

        Zones = new ObservableCollection<Zone>();

        await LoadZonesAsync(cityId);
    }

    private async Task LoadStatesAsync()
    {
        var response = await _repository.GetAsync<List<State>>(StatesUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        States = new ObservableCollection<State>(response.Response ?? new List<State>());
    }

    protected async Task LoadCitiesAsync(int stateId)
    {
        if (stateId <= 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<City>>($"{CitiesUrl}/{stateId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Cities = new ObservableCollection<City>(response.Response ?? new List<City>());
    }

    protected async Task LoadZonesAsync(int cityId)
    {
        if (cityId <= 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<Zone>>($"{ZonesUrl}/{cityId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Zones = new ObservableCollection<Zone>(response.Response ?? new List<Zone>());
    }

    private async Task LoadContractorsAsync()
    {
        var response = await _repository.GetAsync<List<GuidItemModel>>(ContractorsUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Contractors = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    private async Task LoadStatusesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>(StatusUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Statuses = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task LoadStratumsAsync()
    {
        var response = await _repository.GetAsync<List<GuidItemModel>>(StratumUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Stratums = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    //===================== Guardar =====================

    protected async Task<bool> GuardarAsync(bool esEdicion)
    {
        if (!TryValidar(out var mensaje))
        {
            await _alertService.WarningAsync("Validacion", mensaje!);
            return false;
        }

        IsSaving = true;

        try
        {
            //Se manda SOLO lo que el contrato guarda: las navegaciones cargadas harian
            //que el servidor intente crear cliente y contratista de nuevo.
            var modelo = new ContractClientEntity
            {
                ContractClientId = Entity.ContractClientId,
                DateCreado = Entity.DateCreado,
                ControlContrato = Entity.ControlContrato,
                ContractorId = Entity.ContractorId,
                ClientId = Entity.ClientId,
                PhoneNumber = Entity.PhoneNumber,
                PhoneNumber2 = Entity.PhoneNumber2,
                Address = Entity.Address,
                ZoneId = Entity.ZoneId,
                ContractState = Entity.ContractState,
                EquipoEmpres = Entity.EquipoEmpres,
                EnvoiceClient = Entity.EnvoiceClient,
                EstratoSocialId = Entity.EstratoSocialId,
                CorporationId = Entity.CorporationId
            };

            var response = esEdicion
                ? await _repository.PutAsync(BaseUrl, modelo)
                : await _repository.PostAsync(BaseUrl, modelo);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return false;
            }

            await _modalService.CloseAsync(ModalResult.Ok());

            return true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private bool TryValidar(out string? mensaje)
    {
        if (Entity.ClientId == Guid.Empty)
        {
            mensaje = "Debe seleccionar el cliente.";
            return false;
        }

        if (Entity.ContractorId == Guid.Empty)
        {
            mensaje = "Debe seleccionar el contratista.";
            return false;
        }

        if (Entity.ZoneId == Guid.Empty)
        {
            mensaje = "Debe seleccionar el estado, la ciudad y la zona.";
            return false;
        }

        //Sin estado elegido no se manda nada al servidor
        if (Entity.ContractState == 0)
        {
            mensaje = _languageService.Text("Validation_SelectStatus");
            return false;
        }

        mensaje = null;
        return true;
    }

    // Mientras se rearman los combos de una edicion, los eventos del ComboBox no pueden
    // limpiar lo que se acaba de cargar. Es la misma bandera de Zonas y Proveedores.
    protected async Task CargarSinLimpiarAsync(Func<Task> carga)
    {
        _cargando = true;

        try
        {
            await carga();
        }
        finally
        {
            _cargando = false;
        }
    }
}

// Crear un contrato
public partial class CreateContractClientDialogViewModel : ContractClientFormViewModel
{
    public CreateContractClientDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        LanguageService languageService)
        : base(repository, responseHandler, modalService, alertService, languageService)
    {
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        IsEdit = false;

        try
        {
            Entity = new ContractClientEntity();

            await LoadCombosAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await GuardarAsync(false);
    }
}

// Editar un contrato
public partial class EditContractClientDialogViewModel : ContractClientFormViewModel
{
    public EditContractClientDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        LanguageService languageService)
        : base(repository, responseHandler, modalService, alertService, languageService)
    {
    }

    public async Task InitializeAsync(Guid id)
    {
        IsLoading = true;
        IsEdit = true;

        try
        {
            var response = await _repository.GetAsync<ContractClientEntity>($"{BaseUrl}/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            Entity = response.Response ?? new ContractClientEntity();

            await LoadCombosAsync();

            //La cascada se rearma con lo que ya trae el contrato, sin limpiarla
            await CargarSinLimpiarAsync(async () =>
            {
                await LoadCitiesAsync(Entity.StateId);
                await LoadZonesAsync(Entity.CityId);
            });

            ClientFilter = $"{Entity.Client?.FirstName} {Entity.Client?.LastName}".Trim();
            ClientEmail = Entity.Client?.Email;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await GuardarAsync(true);
    }
}
