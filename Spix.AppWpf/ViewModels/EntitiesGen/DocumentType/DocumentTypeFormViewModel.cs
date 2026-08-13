using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.HttpService;
using DocumentTypeEntity = Spix.Domain.EntitiesGen.DocumentType;

namespace Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;

// Reune el estado y las operaciones que Create y Edit comparten para Tipo Documento.
public abstract partial class DocumentTypeFormViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/documenttypes";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private DocumentTypeEntity _documentType = new()
    {
        Active = true
    };

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isLoading;

    protected DocumentTypeFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
    {
        _repository = repository;
        _modalService = modalService;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    // Carga el registro antes de editarlo usando el mismo GET individual de Blazor.
    public async Task LoadAsync(Guid id)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var responseHttp = await _repository.GetAsync<DocumentTypeEntity>(
                $"{BaseUrl}/{id}");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            DocumentType = responseHttp.Response ?? new DocumentTypeEntity
            {
                Active = true
            };
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync(
                "Error de conexion",
                exception.Message);
            await _modalService.CloseAsync(ModalResult.Cancel());
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Ejecuta POST o PUT sin cambiar el contrato del controlador existente.
    protected async Task SaveChangesAsync(bool isEdit)
    {
        if (IsSaving)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(DocumentType.DocumentName))
        {
            await _alertService.WarningAsync(
                "Campo requerido",
                "Debes ingresar el tipo de documento.");
            return;
        }

        IsSaving = true;

        try
        {
            var responseHttp = isEdit
                ? await _repository.PutAsync(BaseUrl, DocumentType)
                : await _repository.PostAsync(BaseUrl, DocumentType);

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync(
                "Error de conexion",
                exception.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Cierra sin guardar y devuelve Cancel al indice que abrio el modal.
    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
