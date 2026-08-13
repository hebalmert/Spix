using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesInven.Purchase;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Purchase;

// Coordina las lineas y el cierre de una compra sin mover sus reglas al cliente WPF.
public partial class PurchaseDetailsViewModel : ObservableObject
{
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private Guid _purchaseId;

    [ObservableProperty]
    private Spix.Domain.EntitiesInven.Purchase? _purchase;

    [ObservableProperty]
    private ObservableCollection<Spix.Domain.EntitiesInven.PurchaseDetail> _details = new();

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

    public bool CanManageDetails => Purchase?.Status == PurchaseStatus.Pendiente;

    public event EventHandler? BackRequested;

    public PurchaseDetailsViewModel(
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

    partial void OnPurchaseChanged(Spix.Domain.EntitiesInven.Purchase? value)
    {
        OnPropertyChanged(nameof(CanManageDetails));
    }

    // Carga una pagina de lineas para evitar descargar compras extensas de una sola vez.
    public async Task LoadAsync(Guid purchaseId, int page = 1)
    {
        if (purchaseId == Guid.Empty)
        {
            return;
        }

        _purchaseId = purchaseId;
        IsLoading = true;
        Message = string.Empty;

        try
        {
            var purchaseResponse = await _repository.GetAsync<Spix.Domain.EntitiesInven.Purchase>(
                $"api/v1/purchases/{purchaseId}");
            if (await _responseHandler.HandleErrorAsync(purchaseResponse))
            {
                return;
            }

            var url = $"api/v1/purchaseDetails?guidId={purchaseId}&page={page}&recordsnumber={PageSize}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter.Trim())}";
            }

            var detailResponse = await _repository.GetAsync<List<Spix.Domain.EntitiesInven.PurchaseDetail>>(url);
            if (await _responseHandler.HandleErrorAsync(detailResponse))
            {
                return;
            }

            Purchase = purchaseResponse.Response;
            Details = new ObservableCollection<Spix.Domain.EntitiesInven.PurchaseDetail>(
                detailResponse.Response ?? new List<Spix.Domain.EntitiesInven.PurchaseDetail>());
            CurrentPage = page;

            detailResponse.HttpResponseMessage.Headers.TryGetValues(
                "Totalpages",
                out var pageHeaders);

            _ = int.TryParse(pageHeaders?.FirstOrDefault(), out var totalPages);
            TotalPages = Math.Max(0, totalPages);

            if (Details.Count == 0)
            {
                Message = "No hay productos registrados en esta compra.";
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

    // Repite la consulta desde la primera pagina cuando el usuario busca una linea.
    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync(_purchaseId, 1);
    }

    // Limpia el criterio para recuperar el detalle completo de la compra.
    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync(_purchaseId, 1);
    }

    // Avanza entre paginas sin descargar las lineas restantes.
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
        {
            return;
        }

        await LoadAsync(_purchaseId, page);
    }

    // Agrega una nueva linea solamente mientras la compra esta pendiente.
    [RelayCommand]
    private async Task NewDetailAsync()
    {
        if (!CanManageDetails || _purchaseId == Guid.Empty)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["PurchaseId"] = _purchaseId
        };

        var result = await _modalService.ShowAsync<CreatePurchaseDetailDialogView>(
            "Crear item compra",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_purchaseId, CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El producto fue agregado a la compra.");
    }

    // Permite ajustar una linea antes de cerrar la compra.
    [RelayCommand]
    private async Task EditDetailAsync(Spix.Domain.EntitiesInven.PurchaseDetail? detail)
    {
        if (!CanManageDetails || detail is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = detail.PurchaseDetailId
        };

        var result = await _modalService.ShowAsync<EditPurchaseDetailDialogView>(
            "Editar item compra",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(_purchaseId, CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El item de compra fue actualizado correctamente.");
    }

    // Elimina una linea pendiente y recarga el total calculado por el Backend.
    [RelayCommand]
    private async Task DeleteDetailAsync(Spix.Domain.EntitiesInven.PurchaseDetail? detail)
    {
        if (!CanManageDetails || detail is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar item compra",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync(
            $"api/v1/purchaseDetails/{detail.PurchaseDetailId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(_purchaseId, CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El item de compra fue eliminado correctamente.");
    }

    // Delega al Backend el cierre que bloquea la compra y actualiza existencias.
    [RelayCommand]
    private async Task CloseAsync()
    {
        if (!CanManageDetails || Purchase is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Cerrar compra",
            "Al cerrar no podras editarla y el inventario sera actualizado.",
            "Cerrar compra");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.PostAsync(
            "api/v1/purchaseDetails/CerrarPurchase",
            Purchase);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(Purchase.PurchaseId, CurrentPage);
        await _alertService.SuccessAsync("Compra cerrada", "El inventario fue actualizado correctamente.");
    }

    // Solicita regresar al listado principal sin acoplar este ViewModel a la ventana.
    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
