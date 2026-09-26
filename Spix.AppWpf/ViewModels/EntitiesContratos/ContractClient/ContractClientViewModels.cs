using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesContratos.ContractClient;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ContractClientEntity = Spix.Domain.EntitiesContratos.ContractClient;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;

// Una fila del listado de contratos, YA LISTA PARA PINTAR.
//
// Aqui se resuelve que botones tiene la fila, porque no todos salen siempre: aprobar solo
// cuando el contrato cumple los requisitos, y pedir la firma solo mientras falte firmar.
// La pantalla no decide nada de eso.
public class ContractClientRow
{
    public ContractClientEntity Item { get; }

    public long Number => Item.ControlContrato;

    public string ClientName => $"{Item.Client?.FirstName} {Item.Client?.LastName}".Trim();

    public string ContractorName => $"{Item.Contractor?.FirstName} {Item.Contractor?.LastName}".Trim();

    // Del nombre del estrato solo interesa el numero: "Estrato 3" -> "3"
    public string Stratum { get; }

    public bool HasEquipment => Item.EquipoEmpres;

    public bool HasInvoice => Item.EnvoiceClient;

    public string StatusText { get; }

    public Brush StatusColor { get; }

    // Aprobar: solo en Pendiente de aprobacion y con fotos + Consentimiento + Contrato firmados
    public bool CanApprove =>
        Item.ContractState == ContractState.PendingApproval &&
        Item.RequirementsComplete;

    // Pedir la firma: solo mientras falte firmar el Consentimiento o el Contrato
    public bool CanRequestSignature => !Item.SignaturesComplete;

    // El mismo boton crea o edita las fotos del documento, segun si ya las tiene
    public bool HasIdPics => Item.TieneIDPic;

    public string IdPicsTooltip => HasIdPics ? "Editar las fotos del documento" : "Cargar las fotos del documento";

    public ContractClientRow(ContractClientEntity item, IReadOnlyDictionary<int, string> estados)
    {
        Item = item;

        Stratum = NumeroDeEstrato(item.EstratoSocial?.EstratoSocialName);

        //El nombre del estado lo manda el backend traducido, en la misma lista del filtro
        StatusText = estados.TryGetValue((int)item.ContractState, out var nombre)
            ? nombre
            : item.ContractState.ToString();

        StatusColor = ContractColors.Pincel(item.ContractState);
    }

    private static string NumeroDeEstrato(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "-";
        }

        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return partes.FirstOrDefault(x => int.TryParse(x, out _)) ?? "-";
    }
}

// Contratos de los clientes, replicado de la pantalla /contractclients de la web.
//
// El contrato no es un registro suelto: es un expediente. De cada fila cuelgan las fotos
// del documento, el Consentimiento, el Contrato, su bitacora y el paso a produccion. Por
// eso la fila tiene siete botones y no dos.
public partial class ContractClientIndexViewModel : PagedListViewModel<ContractClientEntity>
{
    private const string BaseUrl = "api/v1/contractclients";
    private const string DocumentsUrl = "api/v1/contractdocuments";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly LanguageService _languageService;

    //El estado elegido viaja como "id", que es como lo lee este controlador
    protected override string Endpoint => StatusFilter > 0
        ? $"{BaseUrl}?id={StatusFilter}"
        : BaseUrl;

    [ObservableProperty]
    private ObservableCollection<ContractClientRow> _rows = new();

    [ObservableProperty]
    private ObservableCollection<ContractStateOption> _statuses = new();

    private int _statusFilter;

    public int StatusFilter
    {
        get => _statusFilter;
        set
        {
            if (_statusFilter == value)
            {
                return;
            }

            _statusFilter = value;
            OnPropertyChanged();

            _ = LoadAsync(1);
        }
    }

    private readonly Dictionary<int, string> _nombresDeEstado = new();

    public ContractClientIndexViewModel(
        IPagedEntityService<ContractClientEntity> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler,
        LanguageService languageService)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
        _languageService = languageService;

        ArmarEstados();
    }

    // Cada carga arma las filas ya resueltas para la pantalla
    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<ContractClientRow>(
            Items.Select(x => new ContractClientRow(x, _nombresDeEstado)));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateContractClientDialogView>("Crear contrato");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El contrato fue creado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.Item.ContractClientId
        };

        var result = await _modalService.ShowAsync<EditContractClientDialogView>(
            $"Editar contrato {fila.Number}", parametros);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El contrato fue actualizado correctamente.");
    }

    // Las fotos del documento del cliente. El MISMO boton crea o edita, segun si ya las
    // tiene, igual que en la web.
    [RelayCommand]
    private async Task IdPicsAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = fila.Item.ContractClientId
        };

        if (fila.HasIdPics && fila.Item.ContractIDPic is not null)
        {
            parametros["Id"] = fila.Item.ContractIDPic.ContractIDPicId;
        }

        var result = await _modalService.ShowAsync<ContractIdPicsDialogView>(
            $"Documento del cliente · Contrato {fila.Number}", parametros);

        if (result.Succeeded)
        {
            await LoadAsync(CurrentPage);
        }
    }

    // El Consentimiento y el Contrato son el mismo modal con otra plantilla: se genera el
    // PDF en el servidor y, si todavia no esta firmado, se firma alli mismo.
    [RelayCommand]
    private async Task ConsentAsync(ContractClientRow? fila)
    {
        await AbrirDocumentoAsync(fila, ContractDocumentType.ConsentData, "Consentimiento");
    }

    [RelayCommand]
    private async Task ContractAsync(ContractClientRow? fila)
    {
        await AbrirDocumentoAsync(fila, ContractDocumentType.Contract, "Contrato");
    }

    private async Task AbrirDocumentoAsync(ContractClientRow? fila, ContractDocumentType tipo, string titulo)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = fila.Item.ContractClientId,
            ["DocumentType"] = tipo
        };

        var result = await _modalService.ShowAsync<ContractDocumentDialogView>(
            $"{titulo} · Contrato {fila.Number}", parametros);

        if (result.Succeeded)
        {
            await LoadAsync(CurrentPage);
        }
    }

    // Pasar el contrato a produccion. El boton solo sale cuando ya cumple: fotos del
    // documento, Consentimiento y Contrato firmados.
    [RelayCommand]
    private async Task ApproveAsync(ContractClientRow? fila)
    {
        if (fila is null || !fila.CanApprove)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Aprobar contrato",
            "Ya tiene las fotos del documento, el Consentimiento y el Contrato firmados. Desea pasarlo a En proceso?",
            "Aprobar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.PutAsync($"{BaseUrl}/approve/{fila.Item.ContractClientId}", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);

        await _alertService.SuccessAsync("Aprobado", "El contrato paso a En proceso.");
    }

    // Le manda al cliente el correo con el enlace para entrar y firmar sus documentos
    [RelayCommand]
    private async Task RequestSignatureAsync(ContractClientRow? fila)
    {
        if (fila is null || !fila.CanRequestSignature)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Enviar solicitud de firma",
            "Se enviara al correo del cliente el enlace para entrar y firmar sus documentos. Desea enviarlo?",
            "Enviar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.PostAsync($"{DocumentsUrl}/request-signature/{fila.Item.ContractClientId}", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await _alertService.SuccessAsync("Enviado", "El cliente recibio la solicitud de firma en su correo.");
    }

    // La bitacora del contrato: creacion, estados, firmas, suspensiones y exoneraciones.
    // No sale de esta tabla, sale de ContractAudit.
    [RelayCommand]
    private async Task AuditAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = fila.Item.ContractClientId
        };

        await _modalService.ShowAsync<ContractAuditDialogView>(
            $"Contrato {fila.Number}", parametros);
    }

    [RelayCommand]
    private async Task DeleteAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar contrato",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.Item.ContractClientId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);

        await _alertService.SuccessAsync("Eliminado", "El contrato fue eliminado correctamente.");
    }

    // El filtro de la web NO ofrece los ocho estados: solo los tres en los que el contrato
    // todavia se esta armando. Los demas se ven desde Control de contratos.
    private void ArmarEstados()
    {
        //Los nombres salen del MISMO archivo de traducciones del Backend y la web
        var opciones = new List<ContractStateOption>
        {
            new(0, _languageService.Text("Filter_AllStatus")),
            new((int)ContractState.Draft, NombreDeEstado(ContractState.Draft)),
            new((int)ContractState.PendingApproval, NombreDeEstado(ContractState.PendingApproval)),
            new((int)ContractState.InProgress, NombreDeEstado(ContractState.InProgress))
        };

        Statuses = new ObservableCollection<ContractStateOption>(opciones);

        //Para la insignia hacen falta los ocho, no solo los tres del filtro
        foreach (ContractState estado in Enum.GetValues<ContractState>())
        {
            _nombresDeEstado[(int)estado] = NombreDeEstado(estado);
        }
    }

    private string NombreDeEstado(ContractState estado)
    {
        return _languageService.Text($"ContractState_{estado}");
    }
}

// Una opcion del filtro de estado
public class ContractStateOption
{
    public int Value { get; }

    public string Name { get; }

    public ContractStateOption(int value, string name)
    {
        Value = value;
        Name = name;
    }
}

// El color de cada estado del contrato, igual que el StatusBadge de la web.
// Los valores viven en Colors.xaml: aqui solo se dice cual le toca a cada uno.
public static class ContractColors
{
    public static Brush Pincel(ContractState estado)
    {
        var clave = estado switch
        {
            ContractState.Draft => "BrushContractDraft",
            ContractState.PendingApproval => "BrushContractPendingApproval",
            ContractState.InProgress => "BrushContractInProgress",
            ContractState.Active => "BrushContractActive",
            ContractState.Exempt => "BrushContractExempt",
            ContractState.Suspended => "BrushContractSuspended",
            ContractState.Cancelled => "BrushContractCancelled",
            ContractState.Terminated => "BrushContractTerminated",
            _ => "BrushContractUnknown"
        };

        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}
