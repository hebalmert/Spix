using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;

public partial class ContractPppoeDialogViewModel : ObservableObject
{
    private const string Url = "api/v2/contractmksetup/pppoe";
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;
    private ContractPppoeLocalSetupDTO? _setup;

    [ObservableProperty] private string _serverName = string.Empty;
    [ObservableProperty] private string _clientIp = string.Empty;
    [ObservableProperty] private string _profileName = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _saveText = "Crear credencial";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;

    //El estado del acceso: el gemelo del "Tipo Acceso" del IpBinding en HotSpot.
    //La lista llega ARMADA del backend, con su neutro y sin la opcion Corte.
    [ObservableProperty] private ObservableCollection<IntItemModel> _accessStates = new();

    [ObservableProperty] private int _accessStateValue = (int)PppoeAccessState.Activo;

    //Solo al editar se puede cambiar: una credencial recien creada nace Activa
    [ObservableProperty] private bool _isEdit;

    //En Corte por mora el estado lo maneja la suspension, no el operador
    [ObservableProperty] private bool _isCut;

    public bool CanChangeAccess => IsEdit && !IsCut;

    partial void OnIsEditChanged(bool value) => OnPropertyChanged(nameof(CanChangeAccess));

    partial void OnIsCutChanged(bool value) => OnPropertyChanged(nameof(CanChangeAccess));

    //Con que se arma el usuario que propone el sistema. Los manda el detalle del contrato;
    //el DTO del setup no los trae y no hace falta tocar el API para eso.
    public string? ClientLastName { get; set; }

    public string? ControlContrato { get; set; }

    public ContractPppoeDialogViewModel(IRepository repository, HttpResponseHandler responseHandler,
        ModalService modalService, AlertService alertService, ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _mikrotikService = mikrotikService;
    }

    public async Task InitializeAsync(Guid contractClientId, bool edit)
    {
        IsLoading = true;
        try
        {
            var response = await _repository.GetAsync<ContractPppoeLocalSetupDTO>($"{Url}/{contractClientId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            _setup = response.Response;
            if (_setup == null || (edit != _setup.CredentialId.HasValue))
            {
                await _alertService.WarningAsync("Credencial PPPoE", "El estado de la credencial cambio. Actualice el detalle.");
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            ServerName = _setup.ServerName ?? string.Empty;
            ClientIp = _setup.ClientIp ?? string.Empty;
            ProfileName = _setup.ProfileName ?? string.Empty;
            //Al crear se propone; al editar se muestra el que ya tiene
            Username = edit
                ? _setup.CurrentUsername ?? string.Empty
                : PppoeCredential.ProponerUsuario(ClientLastName, ControlContrato);
            Password = edit
                ? _setup.CurrentPassword ?? string.Empty
                : PppoeCredential.GenerarClave();
            SaveText = edit ? "Guardar credencial" : "Crear credencial";

            IsEdit = edit;
            IsCut = _setup.AccessState == PppoeAccessState.Corte;
            AccessStateValue = (int)(edit ? _setup.AccessState : PppoeAccessState.Activo);

            //La lista solo hace falta cuando hay combo que pintar
            if (CanChangeAccess)
            {
                var estados = await _repository.GetAsync<List<IntItemModel>>("api/v2/contractmksetup/pppoeaccessstates");
                if (!await _responseHandler.HandleErrorAsync(estados))
                {
                    AccessStates = new ObservableCollection<IntItemModel>(estados.Response ?? new());
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    //Devuelve el usuario a lo que el sistema propone, igual que el boton # del Blazor
    [RelayCommand]
    private void ProposeUsername()
    {
        Username = PppoeCredential.ProponerUsuario(ClientLastName, ControlContrato);
    }

    [RelayCommand]
    private void GeneratePassword()
    {
        Password = PppoeCredential.GenerarClave();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var setup = _setup;
        if (setup == null) return;

        var username = Username.Trim().ToLowerInvariant();
        var password = Password.Trim();
        if (!PppoeCredential.EsUsuarioValido(username))
        {
            await _alertService.WarningAsync("Credencial PPPoE", PppoeCredential.ReglaTexto);
            return;
        }

        if (password.Length is < 1 or > 50)
        {
            await _alertService.WarningAsync("Credencial PPPoE", "La clave debe tener entre 1 y 50 caracteres.");
            return;
        }

        if (CanChangeAccess &&
            AccessStateValue != (int)PppoeAccessState.Activo &&
            AccessStateValue != (int)PppoeAccessState.Bloqueado)
        {
            await _alertService.WarningAsync("Credencial PPPoE", "Elija el estado de acceso.");
            return;
        }

        if (string.IsNullOrWhiteSpace(setup.ServerIp) || string.IsNullOrWhiteSpace(setup.ClientIp) ||
            string.IsNullOrWhiteSpace(setup.ProfileName))
        {
            await _alertService.WarningAsync("Credencial PPPoE", "Faltan el servidor, la IP o el perfil PPPoE.");
            return;
        }

        IsSaving = true;
        try
        {
            var server = new Server
            {
                ServerId = setup.ServerId,
                ServerName = setup.ServerName,
                Usuario = setup.ServerUser,
                Clave = setup.ServerPassword,
                ApiPort = setup.ApiPort,
                IpNetwork = new IpNetwork { Ip = setup.ServerIp }
            };

            var routerId = setup.MikrotikId;
            var result = await _mikrotikService.ExecuteAsync(server, router =>
            {
                if (setup.CredentialId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(routerId) || string.IsNullOrWhiteSpace(setup.CurrentUsername))
                    {
                        throw new InvalidOperationException("La credencial no tiene identificador MikroTik.");
                    }

                    LocalPppoeCommands.Update(router, routerId, setup.CurrentUsername, username,
                        password, setup.ClientIp);

                    //El estado del acceso. SetAccess deshabilita el secret Y tumba el tunel:
                    //deshabilitarlo solo impide la proxima autenticacion, no bota al que ya
                    //esta adentro. En Corte no se toca: eso lo maneja la suspension.
                    if (!IsCut)
                    {
                        LocalPppoeCommands.SetAccess(router, routerId, username, setup.ClientIp,
                            AccessStateValue == (int)PppoeAccessState.Activo);
                    }
                }
                else
                {
                    routerId = LocalPppoeCommands.Add(router, username, password,
                        setup.ProfileName, setup.ClientIp, setup.ClientName ?? username);
                }
            });

            if (!result.WasExecuted)
            {
                await _alertService.ErrorAsync("Credencial PPPoE", result.Message);
                return;
            }

            var save = new ContractPppoeLocalSaveDTO
            {
                ContractClientId = setup.ContractClientId,
                CredentialId = setup.CredentialId,
                Username = username,
                Password = password,
                MikrotikId = routerId ?? string.Empty,
                AccessState = IsEdit ? (PppoeAccessState)AccessStateValue : PppoeAccessState.Activo
            };

            var response = await _repository.PostAsync<ContractPppoeLocalSaveDTO, ContractPppoe>(Url, save);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _alertService.WarningAsync("Credencial PPPoE",
                    "El MikroTik respondio, pero no se pudo confirmar el registro en Spix. Revise ambos antes de reintentar.");
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
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
}
