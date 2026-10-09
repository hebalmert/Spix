using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.MkDTOs;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesNet.ServerPage;

public partial class DetailedIndexServer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string baseUrlServers = "api/v1/servers";
    //La IP local del PPPoE sale del pozo de IP de RED, no del de clientes
    private const string baseUrlIpNets = "api/v1/ipnetworks";
    private const string baseUrlMk = "api/v1/mkconnections";

    private string Filter { get; set; } = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;

    public List<ServerListItemDto>? Servers { get; set; }
    public Guid? SelectedServerId { get; set; }

    //El servidor COMPLETO del panel derecho: se necesita entero para poder guardarlo
    public Server? Selected { get; set; }
    public bool IsLoadingDetail { get; set; }

    //Las tres listas del panel derecho, todas armadas en el backend
    public List<IntItemModel>? ControlTypes { get; set; }
    public List<MkInterfaceDTO>? Interfaces { get; set; }
    public List<IpNetwork>? LocalIps { get; set; }

    public bool? Connected { get; set; }
    private string ConnectionText { get; set; } = "Conexion sin probar";

    //La identidad del equipo y lo que se conto, por separado: antes iban pegados en un
    //solo texto ("MikroTik - 0") donde no se sabia que era el numero.
    private string? MkName { get; set; }
    private int? MkCount { get; set; }

    //El MISMO numero cuenta cosas distintas segun como trabaje el equipo: secrets en
    //PPPoE, ip-bindings en HotSpot. Sin control definido no cuenta nada util, y por eso
    //devuelve null: la franja no lo muestra.
    private string? MkCountLabel => Selected?.ControlMk switch
    {
        MikrotikControlType.PPPoE => Localizer[nameof(Resource.Server_CountPppoe)],
        MikrotikControlType.HotSpot => Localizer[nameof(Resource.Server_CountBinding)],
        _ => null
    };
    private string? ServiceNameInput { get; set; }

    //Lo ultimo que se guardo, para saber si hay cambios pendientes. Se compara por una
    //clave de los campos editables y no campo por campo: es una pantalla, no un ORM.
    private string? _guardado;

    private string ClaveActual => Selected == null
        ? string.Empty
        : $"{Selected.WanName}|{Selected.LanName}|{(int)Selected.ControlMk}|{Selected.PppLocalIpNetId}|{Selected.PppServiceName}";

    private bool HayCambios => Selected != null && _guardado != ClaveActual;

    //Aviso en pantalla: la guarda de verdad la hace el backend
    private bool ControlMkLocked => SelectedClients > 0;

    // ---------- Lo que pinta la pantalla, ya calculado aca ----------

    private int PppoeCount => Servers?.Count(x => x.ControlMk == MikrotikControlType.PPPoE) ?? 0;

    private int HotSpotCount => Servers?.Count(x => x.ControlMk == MikrotikControlType.HotSpot) ?? 0;

    private int ClientsCount => Servers?.Sum(x => x.Clients) ?? 0;

    private int SelectedClients => Servers?.FirstOrDefault(x => x.ServerId == SelectedServerId)?.Clients ?? 0;

    private bool PppoeReady => !string.IsNullOrWhiteSpace(Selected?.PppServerMkId);

    private string PppoeText => PppoeReady
        ? $"{Selected!.LanName} / {Selected.PppServiceName} / {Selected.PppProfileName}"
        : "Servidor PPPoE sin crear";

    private Guid LocalIpValue => Selected?.PppLocalIpNetId ?? Guid.Empty;

    //El primer paso que falta de la cascada. Null cuando ya se puede crear.
    private string? PppoeBlockedReason
    {
        get
        {
            if (Selected == null) return null;

            if (string.IsNullOrWhiteSpace(Selected.LanName)) return Localizer[nameof(Resource.Server_LanNameRequired)];

            if (Selected.PppLocalIpNetId == null || Selected.PppLocalIpNetId == Guid.Empty) return Localizer[nameof(Resource.Server_PppLocalIpRequired)];

            return null;
        }
    }

    //El nombre traducido del tipo sale de la lista que armo el backend, no de un switch aca
    private string ControlName(MikrotikControlType tipo)
    {
        return ControlTypes?.FirstOrDefault(x => x.Value == (int)tipo)?.Name ?? tipo.ToString();
    }

    // ---------- Carga ----------

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadControlTypesAsync();
            await LoadServersAsync();
        }
    }

    private async Task LoadControlTypesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{baseUrlServers}/controlTypes");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        ControlTypes = responseHttp.Response;
    }

    private async Task LoadServersAsync(int page = 1)
    {
        var url = $"{baseUrlServers}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await _repository.GetAsync<List<ServerListItemDto>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Servers = responseHttp.Response;

        //Si el header no viene, la pantalla sigue andando con una sola pagina
        if (responseHttp.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var paginas) &&
            int.TryParse(paginas.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadServersAsync(page);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await LoadServersAsync();
    }

    //Al elegir un equipo se trae entero, mas las IPs libres para su IP local.
    //Las interfaces NO se leen aca: eso exige conectarse al equipo y colgaria la pantalla
    //si el router no responde. Se leen al probar la conexion.
    //La IP del equipo solo se muestra tapada en la lista: 12.***.***.***
    //Quien necesite la completa entra a editar el servidor.
    private static string IpTapada(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return string.Empty;
        }

        var partes = ip.Split('.');

        //Si no tiene forma de IPv4 no se inventa nada: se deja como esta
        return partes.Length == 4 ? $"{partes[0]}.***.***.***" : ip;
    }

    private async Task SelectServerAsync(Guid serverId)
    {
        SelectedServerId = serverId;
        Selected = null;
        Interfaces = null;
        LocalIps = null;
        Connected = null;
        ConnectionText = "Conexion sin probar";
        MkName = null;
        MkCount = null;
        IsLoadingDetail = true;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.GetAsync<Server>($"{baseUrlServers}/{serverId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsLoadingDetail = false;
            return;
        }

        Selected = responseHttp.Response;
        ServiceNameInput = Selected?.PppServiceName;
        _guardado = ClaveActual;

        await LoadLocalIpsAsync();

        IsLoadingDetail = false;
        await InvokeAsync(StateHasChanged);
    }

    //Las IPs libres, mas la que ya tiene el equipo para que el combo la muestre
    private async Task LoadLocalIpsAsync()
    {
        if (SelectedServerId is not Guid serverId) return;

        //Lista propia: el backend decide que se puede elegir sabiendo de que servidor
        //se trata. Aqui no se filtra nada.
        var responseHttp = await _repository.GetAsync<List<IpNetwork>>($"{baseUrlIpNets}/loadComboLocalPpp/{serverId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        LocalIps = responseHttp.Response;
    }

    // ---------- Lo que le pregunta al equipo ----------

    //Una sola accion: prueba que se puede entrar y de paso trae las interfaces reales
    //El ping va contra la red, no contra el API: sirve para saber si el equipo ni
    //siquiera responde antes de culpar al usuario o la clave del API.
    private async Task ShowPingAsync()
    {
        //La IP viene del renglon de la lista, no de la entidad: el Server no la guarda
        //como texto, la arma el DTO del listado. Mismo patron que SelectedClients.
        var host = Servers?.FirstOrDefault(x => x.ServerId == SelectedServerId)?.Ip;
        if (string.IsNullOrWhiteSpace(host)) return;

        var parameters = new Dictionary<string, object>
        {
            { "Host", host },
            { "Title", $"{Localizer["Net_PingTitle", host]}" }
        };

        await _modalService.ShowAsync(typeof(PingModal), parameters);
    }

    //Devuelve si el equipo respondio, para que quien la llame decida si sigue.
    private async Task<bool> CheckConnectionAsync()
    {
        if (SelectedServerId == null) return false;

        Connected = null;
        ConnectionText = "Probando...";
        MkName = null;
        MkCount = null;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.GetAsync<MkConnectionResultDTO>($"{baseUrlMk}/mkchecks/{SelectedServerId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Connected = false;
            ConnectionText = Localizer[nameof(Resource.Mikrotik_Connection_Error)];
            await InvokeAsync(StateHasChanged);
            return false;
        }

        var datos = responseHttp.Response;
        Connected = true;
        ConnectionText = Localizer[nameof(Resource.Server_Connected)];
        MkName = datos?.MikrotikName;
        MkCount = datos?.Value;

        await InvokeAsync(StateHasChanged);
        return true;
    }

    //Envoltorio para el boton: CheckConnectionAsync devuelve bool y un OnClick no lo admite
    private async Task ProbarConexionAsync() => await CheckConnectionAsync();

    private async Task LoadInterfacesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<MkInterfaceDTO>>($"{baseUrlMk}/interfaces/{SelectedServerId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Interfaces = responseHttp.Response;
    }

    //Deja el equipo listo: perfil mas servidor PPPoE. Una sola vez por servidor.
    //Deshace la configuracion del equipo para poder rehacerla, por ejemplo con otra IP
    //local. El backend se niega si el servidor tiene contratos PPPoE.
    private async Task DeletePppoeServerAsync()
    {
        if (Selected == null) return;

        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.Server_PppoeDelete)],
            Text = Localizer[nameof(Resource.Server_PppoeDeleteText)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.Server_PppoeDelete)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true") return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrlMk}/pppoeserver/{Selected.ServerId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_SuccessTitle)],
            Localizer[nameof(Resource.msg_SuccessMessage)],
            SweetAlertIcon.Success);

        //Se recarga para que la pantalla vuelva al estado "sin crear"
        await SelectServerAsync(Selected.ServerId);
        await LoadServersAsync(CurrentPage);
    }

    private async Task CreatePppoeServerAsync()
    {
        if (Selected == null) return;

        //Se guarda primero: el backend lee la interfaz y la IP local de la base, no de la pantalla
        if (!await SaveSelectedAsync(silencioso: true)) return;

        var url = string.IsNullOrWhiteSpace(ServiceNameInput)
            ? $"{baseUrlMk}/pppoeserver/{Selected.ServerId}"
            : $"{baseUrlMk}/pppoeserver/{Selected.ServerId}?serviceName={Uri.EscapeDataString(ServiceNameInput)}";

        //El cuerpo no se usa: el serviceName viaja por query. IRepository pide un modelo.
        var responseHttp = await _repository.PostAsync(url, Selected.ServerId);
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_SuccessTitle)],
            Localizer[nameof(Resource.msg_SuccessMessage)],
            SweetAlertIcon.Success);

        //Se recarga para ver los ids que quedaron guardados
        await SelectServerAsync(Selected.ServerId);
        await LoadServersAsync(CurrentPage);
    }

    // ---------- Guardar el panel derecho ----------

    private async Task SaveSelectedAsync() => await SaveSelectedAsync(false);

    private async Task<bool> SaveSelectedAsync(bool silencioso)
    {
        if (Selected == null) return false;

        var responseHttp = await _repository.PutAsync(baseUrlServers, Selected);
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return false;

        if (!silencioso)
        {
            await _sweetAlert.FireAsync(
                Localizer[nameof(Resource.msg_SuccessTitle)],
                Localizer[nameof(Resource.msg_SuccessMessage)],
                SweetAlertIcon.Success);
        }

        await LoadServersAsync(CurrentPage);

        //Lo guardado pasa a ser el nuevo punto de partida: el boton se apaga
        _guardado = ClaveActual;

        return true;
    }

    // ---------- Los combos: cada uno actualiza su propiedad y nada mas ----------

    private void WanChanged(ChangeEventArgs e)
    {
        if (Selected == null) return;

        Selected.WanName = e.Value?.ToString();
    }

    private void LanChanged(ChangeEventArgs e)
    {
        if (Selected == null) return;

        Selected.LanName = e.Value?.ToString();
    }

    private void ControlChanged(ChangeEventArgs e)
    {
        if (Selected == null) return;

        if (int.TryParse(e.Value?.ToString(), out var valor) && Enum.IsDefined(typeof(MikrotikControlType), valor))
        {
            Selected.ControlMk = (MikrotikControlType)valor;
        }
    }

    private async Task LocalIpChanged(ChangeEventArgs e)
    {
        if (Selected == null) return;

        if (Guid.TryParse(e.Value?.ToString(), out var id) && id != Guid.Empty)
        {
            //El backend manda en Description el equipo que ya tiene esa IP. No se bloquea:
            //se avisa de quien es y decide el operador. Esto no toca nada en IP de red.
            var elegida = LocalIps?.FirstOrDefault(x => x.IpNetworkId == id);

            if (!string.IsNullOrWhiteSpace(elegida?.Description))
            {
                var aviso = await _sweetAlert.FireAsync(new SweetAlertOptions
                {
                    Title = Localizer[nameof(Resource.PppLocalIp)],
                    Text = Localizer["Server_PppLocalIpTaken", elegida!.Ip ?? string.Empty, elegida.Description!],
                    Icon = SweetAlertIcon.Warning,
                    ShowCancelButton = true,
                    ConfirmButtonText = Localizer[nameof(Resource.ButtonContinue)],
                    CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
                });

                //Si no quiere, la seleccion vuelve a como estaba
                if (aviso.IsDismissed || aviso.Value != "true")
                {
                    await InvokeAsync(StateHasChanged);
                    return;
                }
            }

            Selected.PppLocalIpNetId = id;
        }
        else
        {
            Selected.PppLocalIpNetId = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void ServiceNameChanged(ChangeEventArgs e)
    {
        ServiceNameInput = e.Value?.ToString();

        //Tambien al modelo: antes solo viajaba como parametro al crear, asi que el Save
        //no lo persistia y al volver a entrar el nombre se perdia.
        if (Selected != null)
        {
            Selected.PppServiceName = ServiceNameInput;
        }
    }

    // ---------- Los modales del panel izquierdo ----------

    private async Task ShowModalCreateServerAsync() => await ShowModalServerAsync(false, null);

    private async Task ShowModalEditServerAsync(Guid id) => await ShowModalServerAsync(true, id);

    private async Task ShowModalServerAsync(bool isEdit, Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        if (isEdit)
        {
            component = typeof(EditServer);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Server)]}" }
            };
        }
        else
        {
            component = typeof(CreateServer);
            parameters = new Dictionary<string, object>
            {
                { "Title", $"{Localizer[nameof(Resource.Create_Server)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadServersAsync(CurrentPage);

                if (SelectedServerId is Guid actual)
                {
                    await SelectServerAsync(actual);
                }

                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success);
            }
        });
    }

    private async Task DeleteServerAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true") return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrlServers}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        if (SelectedServerId == id)
        {
            SelectedServerId = null;
            Selected = null;
        }

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_DeleteConfirmationTitle)],
            Localizer[nameof(Resource.msg_DeleteConfirmationText)],
            SweetAlertIcon.Success);

        await LoadServersAsync(CurrentPage);
    }
}
