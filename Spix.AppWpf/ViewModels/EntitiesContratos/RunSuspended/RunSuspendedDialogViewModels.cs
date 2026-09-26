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
using RunSuspendedEntity = Spix.Domain.EntitiesContratos.RunSuspended;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.RunSuspended;

// Crear o editar el corte: solo el periodo. El mismo modal sirve para las dos cosas.
public partial class RunSuspendedFormDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/runsuspended";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _months = new();

    [ObservableProperty]
    private int _yearNumber = DateTime.Today.Year;

    [ObservableProperty]
    private int _monthValue;

    [ObservableProperty]
    private bool _isSaving;

    private Guid? _id;

    public string SaveText => _id.HasValue ? "Guardar" : "Crear";

    public RunSuspendedFormDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public void SetId(Guid id) => _id = id;

    public async Task InitializeAsync()
    {
        //Los meses primero: sin la lista el combo no puede resolver el valor elegido
        var meses = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (await _responseHandler.HandleErrorAsync(meses))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Months = new ObservableCollection<IntItemModel>(meses.Response ?? new List<IntItemModel>());

        if (!_id.HasValue)
        {
            //Un corte nuevo se propone para el mes en curso
            MonthValue = DateTime.Today.Month;
            return;
        }

        var response = await _repository.GetAsync<RunSuspendedEntity>($"{BaseUrl}/{_id.Value}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        var modelo = response.Response;

        if (modelo is null)
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        YearNumber = modelo.YearNumber;
        MonthValue = (int)modelo.MonthType;

        OnPropertyChanged(nameof(SaveText));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        //El neutro de la lista vale 0 y no es un mes: sin esto se guardaria un mes invalido
        if (!Enum.IsDefined(typeof(MonthType), MonthValue))
        {
            await _alertService.WarningAsync("Corte general", "Debe seleccionar el mes.");
            return;
        }

        IsSaving = true;

        try
        {
            var modelo = new RunSuspendedEntity
            {
                RunSuspendedId = _id ?? Guid.Empty,
                YearNumber = YearNumber,
                MonthType = (MonthType)MonthValue
            };

            var response = _id.HasValue
                ? await _repository.PutAsync(BaseUrl, modelo)
                : await _repository.PostAsync(BaseUrl, modelo);

            if (await _responseHandler.HandleErrorAsync(response))
            {
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

// El detalle del corte: la revision previa y la EJECUCION.
//
// Aqui esta el trabajo de verdad. El corte le quita el acceso al cliente en el MikroTik, y
// eso lo hace el ESCRITORIO por la red LAN, porque el cliente puede no tener IP publica.
// Por eso la porcion de MikroTik esta replicada aqui adentro.
//
// Va equipo por equipo y cada uno se confirma solo: si uno no responde, lo ya cortado queda
// cortado y el corte sigue abierto para continuarlo sin repetir a nadie.
public partial class RunSuspendedDetailDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/runsuspended";
    private const string MkUrl = "api/v2/runsuspendedmk";
    private const int PageSize = 15;

    //El lote de un equipo va en UNA sola conexion, y el tiempo de espera del servicio local
    //cubre toda la operacion: por eso se calcula segun cuantos contratos lleva el lote
    private const int EsperaBaseSegundos = 30;
    private const int EsperaPorContratoSegundos = 2;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private string? _periodText;

    [ObservableProperty]
    private bool _executed;

    //===== La revision previa =====
    [ObservableProperty]
    private bool _hasCheck;

    [ObservableProperty]
    private int _debtors;

    [ObservableProperty]
    private int _toSuspend;

    [ObservableProperty]
    private int _alreadySuspended;

    [ObservableProperty]
    private decimal _debtTotal;

    [ObservableProperty]
    private int _noServer;

    [ObservableProperty]
    private ObservableCollection<CorteCheckServerDto> _servers = new();

    //===== Los ya cortados, cuando el corte ya se ejecuto =====
    [ObservableProperty]
    private ObservableCollection<CorteDetailDto> _details = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    //===== El avance =====
    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _processed;

    [ObservableProperty]
    private int _toProcess;

    [ObservableProperty]
    private string? _currentServer;

    [ObservableProperty]
    private bool _isLoading;

    private Guid _id;
    private List<IntItemModel> _months = new();

    public bool HasBlocked => NoServer > 0;

    public string BlockedText => $"Quedan fuera {NoServer} por no tener IpBinding.";

    public bool CanRun => HasCheck && !Executed && ToSuspend > 0;

    public bool HasProgress => IsRunning || Processed > 0;

    public int Percent => ToProcess <= 0 ? 0 : Math.Min(100, Processed * 100 / ToProcess);

    public string ProgressText => $"{Processed} / {ToProcess} · {Percent}%";

    public RunSuspendedDetailDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _mikrotikService = mikrotikService;
    }

    public void SetId(Guid id) => _id = id;

    partial void OnNoServerChanged(int value)
    {
        OnPropertyChanged(nameof(HasBlocked));
        OnPropertyChanged(nameof(BlockedText));
    }

    partial void OnToSuspendChanged(int value) => OnPropertyChanged(nameof(CanRun));

    partial void OnHasCheckChanged(bool value) => OnPropertyChanged(nameof(CanRun));

    partial void OnExecutedChanged(bool value) => OnPropertyChanged(nameof(CanRun));

    partial void OnIsRunningChanged(bool value) => OnPropertyChanged(nameof(HasProgress));

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

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var meses = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
            if (!await _responseHandler.HandleErrorAsync(meses))
            {
                _months = meses.Response ?? new List<IntItemModel>();
            }

            var response = await _repository.GetAsync<RunSuspendedEntity>($"{BaseUrl}/{_id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var modelo = response.Response;

            if (modelo is null)
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var nombreMes = _months.FirstOrDefault(x => x.Value == (int)modelo.MonthType)?.Name
                            ?? modelo.MonthType.ToString();

            PeriodText = $"{nombreMes} {modelo.YearNumber}";
            Executed = modelo.Executed;

            //Un corte ya ejecutado muestra a quienes corto; uno pendiente, la revision
            if (Executed)
            {
                await CargarDetallesAsync(1);
                return;
            }

            await CargarCheckAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CargarCheckAsync()
    {
        var response = await _repository.GetAsync<CorteCheckDto>($"{BaseUrl}/{_id}/check");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasCheck = false;
            return;
        }

        var datos = response.Response;

        if (datos is null)
        {
            HasCheck = false;
            return;
        }

        Debtors = datos.Debtors;
        ToSuspend = datos.ToSuspend;
        AlreadySuspended = datos.AlreadySuspended;
        DebtTotal = datos.DebtTotal;
        NoServer = datos.NoServer;
        Servers = new ObservableCollection<CorteCheckServerDto>(datos.Servers);
        HasCheck = true;
    }

    private async Task CargarDetallesAsync(int page)
    {
        var response = await _repository.GetAsync<List<CorteDetailDto>>(
            $"{BaseUrl}/{_id}/details?page={page}&recordsnumber={PageSize}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Details = new ObservableCollection<CorteDetailDto>(response.Response ?? new List<CorteDetailDto>());
        CurrentPage = page;

        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await CargarDetallesAsync(page);
    }

    // EJECUTAR EL CORTE, equipo por equipo.
    [RelayCommand]
    private async Task RunAsync()
    {
        if (IsRunning || Executed)
        {
            return;
        }

        IsRunning = true;
        Processed = 0;
        ToProcess = 0;
        CurrentServer = null;

        try
        {
            //Se vuelve a revisar: entre abrir la pantalla y apretar el boton alguien pudo pagar
            await CargarCheckAsync();

            if (!HasCheck)
            {
                return;
            }

            if (ToSuspend == 0)
            {
                await _alertService.WarningAsync("Ejecutar corte",
                    "No hay nada que cortar: todos los contratos activos estan al dia.");

                return;
            }

            var texto = $"Se van a suspender {ToSuspend} contrato(s).";

            if (NoServer > 0)
            {
                texto += $" {BlockedText}";
            }

            var confirmado = await _alertService.ConfirmAsync("Ejecutar corte", texto, "Ejecutar corte");

            if (!confirmado)
            {
                return;
            }

            ToProcess = ToSuspend;

            var cortados = 0;
            var saltados = 0;
            var fuera = new List<CorteRunIssueDto>();
            var servidores = Servers.ToList();

            for (var i = 0; i < servidores.Count; i++)
            {
                var servidor = servidores[i];
                CurrentServer = $"Equipo {i + 1} de {servidores.Count}: {servidor.ServerName}";

                var resultado = await CortarServidorAsync(servidor, fuera);

                if (resultado is null)
                {
                    //Ese equipo se corto: lo de los anteriores ya quedo hecho y el corte
                    //sigue abierto para continuarlo
                    await MostrarResultadoAsync(cortados, saltados, fuera, completo: false);
                    return;
                }

                cortados += resultado.Suspended;
                saltados += resultado.Skipped;

                //Avanza el equipo completo, igual que la web
                Processed += servidor.Contracts;
            }

            CurrentServer = null;

            //El cierre no toca el equipo: se usa el de siempre
            var cierre = await _repository.PostAsync($"{BaseUrl}/{_id}/run/finish", new { });
            if (await _responseHandler.HandleErrorAsync(cierre))
            {
                return;
            }

            await MostrarResultadoAsync(cortados, saltados, fuera, completo: true);
            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsRunning = false;
        }
    }

    // Un equipo: se pide su lote, se le escribe por la LAN y se guarda lo que acepto.
    // Devuelve null si hubo que cortar.
    private async Task<CorteRunResultDto?> CortarServidorAsync(
        CorteCheckServerDto servidor,
        List<CorteRunIssueDto> fuera)
    {
        var setup = await _repository.GetAsync<CorteMkSetupDTO>(
            $"{MkUrl}/{_id}/server/{servidor.ServerId}/setup");

        if (await _responseHandler.HandleErrorAsync(setup))
        {
            return null;
        }

        var datos = setup.Response;

        if (datos is null)
        {
            return null;
        }

        if (!datos.CanRun)
        {
            await _alertService.WarningAsync($"Ejecutar corte · {servidor.ServerName}", datos.Blocked!);
            return null;
        }

        if (datos.Contracts.Count == 0)
        {
            return new CorteRunResultDto();
        }

        //Sin IpBinding no hay como quitarles el acceso: se reportan y no se tocan
        if (datos.UsaHotSpot && datos.SinEquipo)
        {
            foreach (var contrato in datos.Contracts)
            {
                fuera.Add(NuevoProblema(contrato, "No tiene IpBinding: no se le puede quitar el acceso."));
            }

            return new CorteRunResultDto();
        }

        //===== Las ordenes al equipo, por la red LAN =====
        //Sin HotSpot no se toca el equipo: el lote se da por bueno y solo cambia el estado

        var cortados = new List<Guid>();

        if (!datos.UsaHotSpot)
        {
            cortados.AddRange(datos.Contracts.Select(x => x.ContractClientId));
        }
        else
        {
            //Se prueba TODO equipo antes de escribir en ninguno: si uno esta caido, se
            //aborta sin dejar a medio cortar. Es lo que hace la web.
            foreach (var grupo in datos.Bindings.GroupBy(x => x.ServerId))
            {
                var prueba = await _mikrotikService.CheckConnectionAsync(ArmarServidor(grupo.First()));

                if (!prueba.WasConnected)
                {
                    await _alertService.ErrorAsync("No se puede continuar con el corte general",
                        $"El equipo {grupo.First().ServerName} no responde o esta fuera de linea.");

                    return null;
                }
            }

            //Una conexion por equipo, y dentro todos sus contratos
            foreach (var grupo in datos.Bindings.GroupBy(x => x.ServerId))
            {
                var bindings = grupo.ToList();
                var aceptados = new List<Guid>();

                var espera = TimeSpan.FromSeconds(
                    EsperaBaseSegundos + EsperaPorContratoSegundos * bindings.Count);

                var resultado = await _mikrotikService.ExecuteAsync(ArmarServidor(bindings[0]), mikrotik =>
                {
                    foreach (var binding in bindings)
                    {
                        //El acceso queda en regular: el cliente cae en el portal del HotSpot
                        mikrotik.Send("/ip/hotspot/ip-binding/set");
                        mikrotik.Send("=.id=" + binding.MikrotikId);
                        mikrotik.Send("=address=" + binding.IpCliente);
                        mikrotik.Send("=to-address=" + binding.IpCliente);
                        mikrotik.Send("=comment=" + binding.Comentario);
                        mikrotik.Send("=mac-address=" + binding.MacCliente);
                        mikrotik.Send("=server=all");
                        mikrotik.Send("=type=" + datos.TipoRegular, true);
                        mikrotik.Read();

                        aceptados.Add(binding.ContractClientId);
                    }
                }, timeout: espera);

                if (!resultado.WasExecuted)
                {
                    await _alertService.ErrorAsync($"Ejecutar corte · {bindings[0].ServerName}", resultado.Message);
                    return null;
                }

                cortados.AddRange(aceptados);
            }
        }

        if (cortados.Count == 0)
        {
            return new CorteRunResultDto();
        }

        //===== El equipo ya quedo escrito: ahora se guarda el rastro =====

        var guardar = await _repository.PostAsync<CorteMkSaveDTO, CorteRunResultDto>(
            $"{MkUrl}/{_id}/server/{servidor.ServerId}",
            new CorteMkSaveDTO { Suspendidos = cortados.Distinct().ToList() });

        if (await _responseHandler.HandleErrorAsync(guardar))
        {
            return null;
        }

        return guardar.Response ?? new CorteRunResultDto();
    }

    private static Server ArmarServidor(CorteMkBindingDTO binding) => new()
    {
        ServerId = binding.ServerId,
        ServerName = binding.ServerName,
        Usuario = binding.Usuario,
        Clave = binding.Clave,
        ApiPort = binding.ApiPort,
        IpNetwork = new IpNetwork { Ip = binding.ServerIp }
    };

    private static CorteRunIssueDto NuevoProblema(CorteMkContractDTO contrato, string motivo) => new()
    {
        ContractClientId = contrato.ContractClientId,
        ControlContrato = contrato.ControlContrato,
        ClientFullName = contrato.ClientFullName ?? string.Empty,
        Reason = motivo
    };

    private async Task MostrarResultadoAsync(int cortados, int saltados, List<CorteRunIssueDto> fuera, bool completo)
    {
        var lineas = new List<string>
        {
            $"Cortados: {cortados}",
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
                "El corte se interrumpio. Lo cortado quedo cortado: vuelva a lanzarlo para continuar.",
                texto);

            return;
        }

        if (fuera.Count > 0)
        {
            await _alertService.WarningAsync("Corte terminado", texto);
            return;
        }

        await _alertService.SuccessAsync("Corte terminado", texto);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
