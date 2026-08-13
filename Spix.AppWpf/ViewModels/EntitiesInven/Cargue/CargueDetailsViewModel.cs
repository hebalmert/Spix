using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesInven.Cargue;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Cargue;

// Controla los seriales de una recepcion y su cierre sin mover reglas de inventario al escritorio.
public partial class CargueDetailsViewModel : ObservableObject
{
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private Guid _cargueId;

    [ObservableProperty]
    private Spix.Domain.EntitiesInven.Cargue? _cargue;

    [ObservableProperty]
    private ObservableCollection<Spix.Domain.EntitiesInven.CargueDetail> _details = new();

    [ObservableProperty]
    private string _filter = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool CanManageSerials => Cargue?.Status == CargueType.Pendiente;

    public bool CanUploadSerials => CanManageSerials &&
                                    Cargue is not null &&
                                    Cargue.TotalSeriales < Cargue.CantToUp;

    public bool CanCloseCargue => CanManageSerials &&
                                  Cargue is not null &&
                                  Cargue.TotalSeriales == Cargue.CantToUp;

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

    partial void OnCargueChanged(Spix.Domain.EntitiesInven.Cargue? value)
    {
        OnPropertyChanged(nameof(CanManageSerials));
        OnPropertyChanged(nameof(CanUploadSerials));
        OnPropertyChanged(nameof(CanCloseCargue));
    }

    // Carga una pagina de MAC y el encabezado que determina si aun falta por subir.
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
            var cargueResponse = await _repository.GetAsync<Spix.Domain.EntitiesInven.Cargue>(
                $"api/v1/cargues/{cargueId}");
            if (await _responseHandler.HandleErrorAsync(cargueResponse))
            {
                return;
            }

            var url = $"api/v1/cargueDetails?guidId={cargueId}&page={page}&recordsnumber={PageSize}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter.Trim())}";
            }

            var detailsResponse = await _repository.GetAsync<List<Spix.Domain.EntitiesInven.CargueDetail>>(url);
            if (await _responseHandler.HandleErrorAsync(detailsResponse))
            {
                return;
            }

            Cargue = cargueResponse.Response;
            Details = new ObservableCollection<Spix.Domain.EntitiesInven.CargueDetail>(
                detailsResponse.Response ?? new List<Spix.Domain.EntitiesInven.CargueDetail>());
            CurrentPage = page;

            detailsResponse.HttpResponseMessage.Headers.TryGetValues(
                "Totalpages",
                out var pageHeaders);

            _ = int.TryParse(pageHeaders?.FirstOrDefault(), out var totalPages);
            TotalPages = Math.Max(0, totalPages);

            if (Details.Count == 0)
            {
                Message = "Aun no hay seriales cargados.";
            }
        }
        catch (Exception exception)
        {
            Details.Clear();
            TotalPages = 0;
            Message = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync(_cargueId, 1);
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync(_cargueId, 1);
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

    // Abre el formulario de carga solamente cuando el total registrado aun es insuficiente.
    [RelayCommand]
    private async Task UploadSerialAsync()
    {
        if (!CanUploadSerials || _cargueId == Guid.Empty)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["CargueId"] = _cargueId
        };

        var result = await _modalService.ShowAsync<CreateCargueDetailDialogView>(
            "Subir serial",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El serial fue cargado correctamente.");
    }

    [RelayCommand]
    private async Task EditSerialAsync(Spix.Domain.EntitiesInven.CargueDetail? detail)
    {
        if (!CanManageSerials || detail is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = detail.CargueDetailId
        };

        var result = await _modalService.ShowAsync<EditCargueDetailDialogView>(
            "Editar serial",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El serial fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteSerialAsync(Spix.Domain.EntitiesInven.CargueDetail? detail)
    {
        if (!CanManageSerials || detail is null)
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

        var response = await _repository.DeleteAsync(
            $"api/v1/cargueDetails/{detail.CargueDetailId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El serial fue eliminado correctamente.");
    }

    // Cierra el cargue mediante el endpoint existente y evita cambios posteriores desde la interfaz.
    [RelayCommand]
    private async Task CloseAsync()
    {
        if (!CanCloseCargue || Cargue is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Cerrar cargue",
            "Al cerrar no podras editar los seriales de esta recepcion.",
            "Cerrar cargue");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.GetAsync(
            $"api/v1/cargueDetails/CerrarTrans/{Cargue.CargueId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(_cargueId, CurrentPage);
        await _alertService.SuccessAsync("Cargue cerrado", "La recepcion de seriales fue completada.");
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
