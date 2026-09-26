using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.Activation;

// Reactivacion: le devuelve el servicio a los que se cortaron y ya pagaron, equipo por equipo.
//
// Es el proceso inverso del corte. Solo entran los que quedaron suspendidos POR EL CORTE,
// con el pago recibido y sin reactivar todavia; el resto de suspensiones se levantan a mano
// desde Contratos suspendidos.
//
// OJO con donde esta el trabajo: la reactivacion le escribe el acceso al cliente en el
// MikroTik, y eso lo hace el ESCRITORIO por la red LAN, porque el cliente puede no tener IP
// publica. Por eso la porcion de MikroTik esta replicada aqui adentro. El v2 solo entrega el
// lote de cada equipo y guarda despues lo que el equipo acepto.
public partial class ActivationIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/activation";
    private const string MkUrl = "api/v2/activationmk";
    private const int PageSize = 15;

    //El lote de un equipo va en UNA sola conexion, y el tiempo de espera del servicio local
    //cubre toda la operacion: por eso se calcula segun cuantos contratos lleva el lote
    private const int EsperaBaseSegundos = 30;
    private const int EsperaPorContratoSegundos = 2;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private ObservableCollection<ActivationPendingRow> _rows = new();

    [ObservableProperty]
    private ObservableCollection<ActivationServerDto> _servers = new();

    [ObservableProperty]
    private int _ready;

    [ObservableProperty]
    private int _toActivate;

    [ObservableProperty]
    private int _noBinding;

    [ObservableProperty]
    private int _noQueue;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //Mientras corre el lote no se puede volver a lanzar
    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _processed;

    [ObservableProperty]
    private int _toProcess;

    [ObservableProperty]
    private string? _currentServer;

    private bool _checkListo;

    public string Subtitle => "Le devuelve el servicio a los que se cortaron y ya pagaron, equipo por equipo.";

    // El boton se ve si hay alguien listo. Mientras corre NO se esconde, se apaga: que
    // desaparezca a mitad del lote deja al usuario sin saber que esta pasando.
    public bool CanRun => _checkListo && ToActivate > 0;

    // El aviso sale solo si hay contratos a los que les falta configuracion
    public bool HasBlocked => NoBinding > 0 || NoQueue > 0;

    public string BlockedText => "Algunos no se pueden reactivar hasta que les completen el IpBinding o el Queue.";

    public bool HasProgress => IsRunning || Processed > 0;

    public int Percent => ToProcess <= 0 ? 0 : (int)Math.Round(Processed * 100d / ToProcess);

    public string ProgressText => $"{Processed} / {ToProcess} · {Percent}%";

    public ActivationIndexViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        AlertService alertService,
        ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
        _mikrotikService = mikrotikService;
    }

    public async Task InitializeAsync()
    {
        await LoadCheckAsync();
        await LoadPendingAsync(1);
    }

    partial void OnToActivateChanged(int value) => OnPropertyChanged(nameof(CanRun));

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(CanRun));
        OnPropertyChanged(nameof(HasProgress));
    }

    partial void OnNoBindingChanged(int value) => OnPropertyChanged(nameof(HasBlocked));

    partial void OnNoQueueChanged(int value) => OnPropertyChanged(nameof(HasBlocked));

    partial void OnProcessedChanged(int value)
    {
        OnPropertyChanged(nameof(HasProgress));
        OnPropertyChanged(nameof(Percent));
        OnPropertyChanged(nameof(ProgressText));
    }

    partial void OnToProcessChanged(int value)
    {
        OnPropertyChanged(nameof(Percent));
        OnPropertyChanged(nameof(ProgressText));
    }

    // El tablero y el reparto por equipo: lo cuenta la base
    private async Task LoadCheckAsync()
    {
        var response = await _repository.GetAsync<ActivationCheckDto>($"{BaseUrl}/check");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            //Sin el check no se pinta el tablero ni se deja lanzar, pero la tabla si carga
            _checkListo = false;
            OnPropertyChanged(nameof(CanRun));
            return;
        }

        var datos = response.Response ?? new ActivationCheckDto();

        Ready = datos.Ready;
        ToActivate = datos.ToActivate;
        NoBinding = datos.NoBinding;
        NoQueue = datos.NoQueue;
        Servers = new ObservableCollection<ActivationServerDto>(datos.Servers);

        _checkListo = true;
        OnPropertyChanged(nameof(CanRun));
    }

    private async Task LoadPendingAsync(int page)
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ActivationDetailDto>>(
                $"{BaseUrl}/pending?page={page}&recordsnumber={PageSize}");

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<ActivationPendingRow>(
                (response.Response ?? new List<ActivationDetailDto>()).Select(x => new ActivationPendingRow(x)));

            CurrentPage = page;

            //El total de paginas viaja en el encabezado, como en el resto del sistema
            if (response.HttpResponseMessage is not null &&
                response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
                int.TryParse(valores.FirstOrDefault(), out var total))
            {
                TotalPages = total;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCheckAsync();
        await LoadPendingAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadPendingAsync(page);
    }

    // Lanza la reactivacion equipo por equipo.
    //
    // Cada equipo se atiende completo y se confirma solo: si uno falla, lo de los equipos
    // anteriores queda hecho y el lote se puede volver a lanzar. Es la misma regla de la web.
    [RelayCommand]
    private async Task RunAsync()
    {
        if (IsRunning || !_checkListo)
        {
            return;
        }

        IsRunning = true;
        Processed = 0;
        ToProcess = 0;
        CurrentServer = null;

        try
        {
            //Se vuelve a mirar el tablero, por si alguien mas pago o reactivo mientras tanto
            await LoadCheckAsync();

            if (ToActivate == 0)
            {
                await _alertService.WarningAsync("Reactivar", "No hay nadie esperando reactivacion.");
                return;
            }

            var confirmado = await _alertService.ConfirmAsync(
                "Reactivar",
                $"Se les va a devolver el servicio a {ToActivate} contrato(s).",
                "Reactivar");

            if (!confirmado)
            {
                return;
            }

            ToProcess = ToActivate;

            var activados = 0;
            var saltados = 0;
            var fuera = new List<ActivationIssueDto>();
            var servidores = Servers.ToList();

            foreach (var servidor in servidores)
            {
                CurrentServer = servidor.ServerName;

                var resultado = await ReactivarServidorAsync(servidor, fuera);

                if (resultado is null)
                {
                    //Ese equipo se corto: lo de los anteriores ya quedo hecho
                    await MostrarResultadoAsync(activados, saltados, fuera, completo: false);
                    await RefreshAsync();
                    return;
                }

                activados += resultado.Activated;
                saltados += resultado.Skipped;

                //Avanza el equipo completo, igual que la web
                Processed += servidor.Contracts;
            }

            CurrentServer = null;

            await MostrarResultadoAsync(activados, saltados, fuera, completo: true);
            await RefreshAsync();
        }
        finally
        {
            IsRunning = false;
        }
    }

    // Un equipo: se piden sus datos, se le escribe por la LAN y se guarda lo que acepto.
    // Devuelve null si hubo que cortar.
    private async Task<ActivationRunResultDto?> ReactivarServidorAsync(
        ActivationServerDto servidor,
        List<ActivationIssueDto> fuera)
    {
        var setup = await _repository.GetAsync<ActivationMkSetupDTO>(
            $"{MkUrl}/server/{servidor.ServerId}/activate");

        if (await _responseHandler.HandleErrorAsync(setup))
        {
            return null;
        }

        var datos = setup.Response;

        if (datos is null)
        {
            return null;
        }

        if (!datos.CanActivate)
        {
            await _alertService.WarningAsync($"Reactivar · {servidor.ServerName}", datos.Blocked!);
            return null;
        }

        if (datos.Bindings.Count == 0)
        {
            return new ActivationRunResultDto();
        }

        //Las listas viven FUERA de la orden: si el equipo se cae a mitad de camino, hay que
        //saber igual a quienes alcanzo a escribir
        var aceptados = new List<Guid>();
        var problemas = new List<ActivationIssueDto>();

        //===== Las ordenes al equipo, por la red LAN =====
        //Sin HotSpot no se toca el equipo: el lote se da por bueno y solo cambia el estado

        if (!datos.UsaHotSpot)
        {
            aceptados.AddRange(datos.Bindings.Select(x => x.ContractClientId));
        }
        else
        {
            var server = new Server
            {
                ServerId = datos.ServerId,
                ServerName = datos.ServerName,
                Usuario = datos.Usuario,
                Clave = datos.Clave,
                ApiPort = datos.ApiPort,
                IpNetwork = new IpNetwork { Ip = datos.ServerIp }
            };

            var espera = TimeSpan.FromSeconds(
                EsperaBaseSegundos + EsperaPorContratoSegundos * datos.Bindings.Count);

            var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
            {
                foreach (var binding in datos.Bindings)
                {
                    //Al que le falte configuracion no se le manda nada: se anota y sigue
                    if (string.IsNullOrWhiteSpace(binding.IpCliente) ||
                        string.IsNullOrWhiteSpace(binding.MacCliente) ||
                        string.IsNullOrWhiteSpace(binding.MikrotikId))
                    {
                        problemas.Add(NuevoProblema(binding, "Su IpBinding no tiene lo que el equipo necesita."));
                        continue;
                    }

                    try
                    {
                        mikrotik.Send("/ip/hotspot/ip-binding/set");
                        mikrotik.Send("=.id=" + binding.MikrotikId);
                        mikrotik.Send("=address=" + binding.IpCliente);
                        mikrotik.Send("=to-address=" + binding.IpCliente);
                        mikrotik.Send("=comment=" + binding.Comentario);
                        mikrotik.Send("=mac-address=" + binding.MacCliente);
                        mikrotik.Send("=server=all");
                        mikrotik.Send("=type=" + datos.TipoBypassed, true);
                        mikrotik.Read();

                        aceptados.Add(binding.ContractClientId);
                    }
                    catch
                    {
                        //Uno que no respondio no detiene a los demas
                        problemas.Add(NuevoProblema(binding, "El equipo no acepto el cambio: reactivelo desde Contratos Suspendidos."));
                    }
                }
            }, timeout: espera);

            if (!resultado.WasExecuted)
            {
                await _alertService.ErrorAsync($"Reactivar · {servidor.ServerName}", resultado.Message);
                return null;
            }
        }

        fuera.AddRange(problemas);

        if (aceptados.Count == 0)
        {
            return new ActivationRunResultDto();
        }

        //===== El equipo ya quedo escrito: ahora se guarda el rastro =====

        var guardar = await _repository.PostAsync<ActivationMkSaveDTO, ActivationRunResultDto>(
            $"{MkUrl}/server/{servidor.ServerId}/activate",
            new ActivationMkSaveDTO { Activados = aceptados });

        if (await _responseHandler.HandleErrorAsync(guardar))
        {
            return null;
        }

        return guardar.Response ?? new ActivationRunResultDto();
    }

    private static ActivationIssueDto NuevoProblema(ActivationMkBindingDTO binding, string motivo) => new()
    {
        ControlContrato = binding.ControlContrato,
        ClientFullName = binding.ClientFullName ?? string.Empty,
        Reason = motivo
    };

    private async Task MostrarResultadoAsync(int activados, int saltados, List<ActivationIssueDto> fuera, bool completo)
    {
        var lineas = new List<string>
        {
            $"Reactivados: {activados}",
            $"Saltados: {saltados}"
        };

        if (fuera.Count > 0)
        {
            lineas.Add($"Quedaron fuera: {fuera.Count}");
        }

        var texto = string.Join(Environment.NewLine, lineas);

        if (!completo)
        {
            await _alertService.WarningAsync(
                "La reactivacion se interrumpio. Lo reactivado quedo reactivado: vuelva a lanzarla para continuar.",
                texto);

            return;
        }

        if (fuera.Count > 0)
        {
            await _alertService.WarningAsync("Reactivacion terminada", texto);
            return;
        }

        await _alertService.SuccessAsync("Reactivacion terminada", texto);
    }
}

// Una fila de los que esperan, ya lista para pintar
public class ActivationPendingRow
{
    public ActivationDetailDto Item { get; }

    public string ClienteText => $"#{Item.ControlContrato} {Item.ClientFullName}";

    public string? ZoneName => Item.ZoneName;

    public string? ServerName => Item.ServerName;

    public string DateSuspendedText => Item.DateSuspended.ToString("dd/MM/yyyy");

    public string DatePaymentReceivedText => Item.DatePaymentReceived?.ToString("dd/MM/yyyy") ?? string.Empty;

    // Las etiquetas rojas: lo que le falta para poder reactivarse
    public bool FaltaBinding => !Item.HasBinding;

    public bool FaltaQueue => !Item.HasQueue;

    public ActivationPendingRow(ActivationDetailDto item)
    {
        Item = item;
    }
}
