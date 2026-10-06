using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesInven.Transfer;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using TransferEntity = Spix.Domain.EntitiesInven.Transfer;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Transfer;

// Las lineas de un traslado y su cierre. Replica de DetailsTransferDertails de la web.
//
// El cierre lo hace el BACKEND: es el que descuenta de la bodega origen y suma en la
// destino. Aqui no se mueve inventario, solo se pide.
public partial class TransferDetailsViewModel : ObservableObject
{
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private Guid _transferId;

    [ObservableProperty]
    private TransferEntity? _transfer;

    [ObservableProperty]
    private ObservableCollection<TransferDetails> _details = new();

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

    //Un traslado cerrado ya movio el stock: sus lineas no se tocan mas
    public bool CanManageDetails => Transfer?.Status == TransferType.Pendiente;

    public event EventHandler? BackRequested;

    public TransferDetailsViewModel(
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

    partial void OnTransferChanged(TransferEntity? value)
    {
        OnPropertyChanged(nameof(CanManageDetails));
    }

    // Una pagina de lineas por vez, para no bajar un traslado largo de un golpe
    public async Task LoadAsync(Guid transferId, int page = 1)
    {
        if (transferId == Guid.Empty)
        {
            return;
        }

        _transferId = transferId;
        IsLoading = true;
        Message = string.Empty;

        try
        {
            var transferResponse = await _repository.GetAsync<TransferEntity>($"api/v2/transfers/{transferId}");
            if (await _responseHandler.HandleErrorAsync(transferResponse))
            {
                return;
            }

            var url = $"api/v2/transferDetails?guidId={transferId}&page={page}&recordsnumber={PageSize}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter.Trim())}";
            }

            var detailResponse = await _repository.GetAsync<List<TransferDetails>>(url);
            if (await _responseHandler.HandleErrorAsync(detailResponse))
            {
                return;
            }

            Transfer = transferResponse.Response;
            Details = new ObservableCollection<TransferDetails>(
                detailResponse.Response ?? new List<TransferDetails>());
            CurrentPage = page;

            detailResponse.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var pageHeaders);

            _ = int.TryParse(pageHeaders?.FirstOrDefault(), out var totalPages);
            TotalPages = Math.Max(0, totalPages);

            if (Details.Count == 0)
            {
                Message = "No hay productos registrados en este traslado.";
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
        await LoadAsync(_transferId, 1);
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync(_transferId, 1);
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
        {
            return;
        }

        await LoadAsync(_transferId, page);
    }

    [RelayCommand]
    private async Task NewDetailAsync()
    {
        if (!CanManageDetails || _transferId == Guid.Empty)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["TransferId"] = _transferId
        };

        var result = await _modalService.ShowAsync<CreateTransferDetailDialogView>("Crear item traslado", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_transferId, CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El producto fue agregado al traslado.");
    }

    [RelayCommand]
    private async Task EditDetailAsync(TransferDetails? detail)
    {
        if (!CanManageDetails || detail is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = detail.TransferDetailsId
        };

        var result = await _modalService.ShowAsync<EditTransferDetailDialogView>("Editar item traslado", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_transferId, CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El item del traslado fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteDetailAsync(TransferDetails? detail)
    {
        if (!CanManageDetails || detail is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar item traslado",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"api/v2/transferDetails/{detail.TransferDetailsId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(_transferId, CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El item del traslado fue eliminado correctamente.");
    }

    // El cierre lo hace el Backend: descuenta en la bodega origen y suma en la destino
    [RelayCommand]
    private async Task CloseAsync()
    {
        if (!CanManageDetails || Transfer is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Cerrar traslado",
            "Al cerrar no podras editarlo y el inventario sera actualizado.",
            "Cerrar traslado");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.PostAsync("api/v2/transferDetails/CerrarTrans", Transfer);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(Transfer.TransferId, CurrentPage);
        await _alertService.SuccessAsync("Traslado cerrado", "El inventario fue actualizado correctamente.");
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
