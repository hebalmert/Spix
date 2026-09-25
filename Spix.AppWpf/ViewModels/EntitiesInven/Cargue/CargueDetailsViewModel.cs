using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesInven.Cargue;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Cargue;

// El detalle de un cargue: cuanto se lleva subido y cuales son sus seriales.
//
// Lee los MISMOS endpoints que la web (cargueboard), no la tabla en crudo: de ahi salen
// el desglose (disponibles, instalados, averiados) y, por cada MAC, en que contrato quedo
// instalada. Con la entidad pelada esos datos no existen.
public partial class CargueDetailsViewModel : ObservableObject
{
    private const int PageSize = 15;
    private const string BoardUrl = "api/v1/cargueboard";
    private const string DetailsUrl = "api/v1/cargueDetails";

    //El mismo formato que valida la web antes de ir al servidor
    private static readonly Regex FormatoMac = new(@"^([0-9A-Fa-f]{2}[:-]?){5}[0-9A-Fa-f]{2}$");

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    private Guid _cargueId;

    [ObservableProperty]
    private CargueProgressDto? _progress;

    [ObservableProperty]
    private ObservableCollection<CargueSerialDto> _serials = new();

    [ObservableProperty]
    private string _filter = string.Empty;

    //Lo que se teclea o dispara el lector de codigo de barras
    [ObservableProperty]
    private string _scanMac = string.Empty;

    [ObservableProperty]
    private string _scanMessage = string.Empty;

    [ObservableProperty]
    private bool _scanOk;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool HasScanMessage => !string.IsNullOrWhiteSpace(ScanMessage);

    public bool CanManageSerials => Progress?.Status == CargueType.Pendiente;

    public int Missing => Progress is null ? 0 : Math.Max(0, (int)Progress.CantToUp - Progress.Uploaded);

    // Mientras falten MAC se sigue escaneando
    public bool CanUploadSerials => CanManageSerials && Missing > 0;

    // Y solo cuando ya no falta ninguna se puede cerrar
    public bool CanCloseCargue => CanManageSerials && Missing == 0;

    public event EventHandler? BackRequested;

    public CargueDetailsViewModel(
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    partial void OnScanMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasScanMessage));
    }

    partial void OnProgressChanged(CargueProgressDto? value)
    {
        OnPropertyChanged(nameof(CanManageSerials));
        OnPropertyChanged(nameof(CanUploadSerials));
        OnPropertyChanged(nameof(CanCloseCargue));
        OnPropertyChanged(nameof(Missing));
    }

    public async Task LoadAsync(Guid cargueId, int page = 1)
    {
        if (cargueId == Guid.Empty)
        {
            return;
        }

        _cargueId = cargueId;
        IsLoading = true;
        Message = string.Empty;

        try
        {
            var avance = await _repository.GetAsync<CargueProgressDto>($"{BoardUrl}/{cargueId}/progress");
            if (await _responseHandler.HandleErrorAsync(avance))
            {
                return;
            }

            var url = $"{BoardUrl}/{cargueId}/serials?page={page}&recordsnumber={PageSize}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter.Trim())}";
            }

            var seriales = await _repository.GetAsync<List<CargueSerialDto>>(url);
            if (await _responseHandler.HandleErrorAsync(seriales))
            {
                return;
            }

            Progress = avance.Response;
            Serials = new ObservableCollection<CargueSerialDto>(seriales.Response ?? new List<CargueSerialDto>());
            CurrentPage = page;

            seriales.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var cabecera);
            _ = int.TryParse(cabecera?.FirstOrDefault(), out var totalPages);
            TotalPages = Math.Max(0, totalPages);
        }
        catch (Exception exception)
        {
            Serials.Clear();
            TotalPages = 0;
            Message = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Enter del lector: guarda, limpia el campo y lo deja listo para la siguiente MAC.
    // Los avisos van en linea y no en ventana, para no cortar el ritmo del escaneo.
    [RelayCommand]
    private async Task ScanAsync()
    {
        var mac = ScanMac.Trim();

        if (string.IsNullOrEmpty(mac) || !CanUploadSerials)
        {
            return;
        }

        if (!FormatoMac.IsMatch(mac))
        {
            MostrarAviso(false, $"La MAC {mac} no tiene el formato correcto.");
            return;
        }

        var responseHttp = await _repository.PostAsync(DetailsUrl, new CargueDetail
        {
            CargueId = _cargueId,
            MacWlan = mac
        });

        if (responseHttp.Error)
        {
            //Lo que rechaza el negocio (MAC repetida, cargue lleno) se muestra en linea;
            //lo demas (sesion, permisos, servidor) sigue el manejo central.
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var mensaje = await responseHttp.GetErrorMessageAsync();
                MostrarAviso(false, mensaje?.Trim('"') ?? mac);
            }
            else
            {
                await _responseHandler.HandleErrorAsync(responseHttp);
            }

            return;
        }

        MostrarAviso(true, $"{mac} cargada.");
        ScanMac = string.Empty;

        await LoadAsync(_cargueId, CurrentPage);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync(_cargueId);
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync(_cargueId);
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
        {
            return;
        }

        await LoadAsync(_cargueId, page);
    }

    [RelayCommand]
    private async Task EditSerialAsync(CargueSerialDto? serial)
    {
        if (!CanManageSerials || serial is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditCargueDetailDialogView>(
            "Editar serial",
            new Dictionary<string, object> { ["Id"] = serial.CargueDetailId });

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El serial fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteSerialAsync(CargueSerialDto? serial)
    {
        if (!CanManageSerials || serial is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar serial",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{DetailsUrl}/{serial.CargueDetailId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El serial fue eliminado correctamente.");
    }

    // Cerrar el cargue mueve el inventario y ya no se le tocan los seriales.
    [RelayCommand]
    private async Task CloseAsync()
    {
        if (!CanCloseCargue)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Cerrar cargue",
            "Al cerrar no podras editar los seriales de esta recepcion.",
            "Cerrar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.GetAsync($"{DetailsUrl}/CerrarTrans/{_cargueId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Cerrado", "El cargue fue cerrado correctamente.");
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MostrarAviso(bool correcto, string mensaje)
    {
        ScanOk = correcto;
        ScanMessage = mensaje;
    }
}
