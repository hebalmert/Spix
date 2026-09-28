using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;

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
            Username = _setup.CurrentUsername ?? string.Empty;
            Password = _setup.CurrentPassword ?? string.Empty;
            SaveText = edit ? "Guardar credencial" : "Crear credencial";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GeneratePassword()
    {
        Password = Guid.NewGuid().ToString("N")[..10];
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var setup = _setup;
        if (setup == null) return;

        var username = Username.Trim().ToLowerInvariant();
        var password = Password.Trim();
        if (username.Length is < 1 or > 50 || password.Length is < 1 or > 50)
        {
            await _alertService.WarningAsync("Credencial PPPoE", "Usuario y clave deben tener entre 1 y 50 caracteres.");
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
                MikrotikId = routerId ?? string.Empty
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
