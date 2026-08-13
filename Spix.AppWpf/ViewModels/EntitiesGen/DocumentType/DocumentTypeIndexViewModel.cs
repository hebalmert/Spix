using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.DocumentType;
using Spix.HttpService;
using DocumentTypeEntity = Spix.Domain.EntitiesGen.DocumentType;

namespace Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;

// Consulta los tipos de documento usando paginacion desde el endpoint existente.
public partial class DocumentTypeIndexViewModel : PagedListViewModel<DocumentTypeEntity>
{
    protected override string Endpoint => "api/v1/documenttypes";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    public DocumentTypeIndexViewModel(
        IPagedEntityService<DocumentTypeEntity> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    // Abre Create y recarga la pagina actual solo si el formulario confirma el guardado.
    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateDocumentTypeDialogView>(
            "Crear tipo documento");

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync(
            "Guardado",
            "El tipo de documento fue guardado correctamente.");
    }

    // Abre Edit con el identificador que usa el GET individual del controlador.
    [RelayCommand]
    private async Task EditAsync(DocumentTypeEntity? documentType)
    {
        if (documentType is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = documentType.DocumentTypeId
        };

        var result = await _modalService.ShowAsync<EditDocumentTypeDialogView>(
            "Editar tipo documento",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync(
            "Actualizado",
            "El tipo de documento fue actualizado correctamente.");
    }

    // Solicita confirmacion antes de ejecutar el DELETE real del controlador.
    [RelayCommand]
    private async Task DeleteAsync(DocumentTypeEntity? documentType)
    {
        if (documentType is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar tipo documento",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        try
        {
            var responseHttp = await _repository.DeleteAsync(
                $"{Endpoint}/{documentType.DocumentTypeId}");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            await LoadAsync(CurrentPage);
            await _alertService.SuccessAsync(
                "Eliminado",
                "El tipo de documento fue eliminado correctamente.");
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync(
                "Error de conexion",
                exception.Message);
        }
    }
}
