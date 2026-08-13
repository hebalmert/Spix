using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesMK.QueueType;
using QueueTypeEntity = Spix.Domain.EntitiesMK.QueueType;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesMK.QueueType;

// Mantiene el CRUD paginado de tipos de Queue disponible en la aplicacion web.
public partial class QueueTypeIndexViewModel : PagedListViewModel<QueueTypeEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/queuetypes";

    public QueueTypeIndexViewModel(IPagedEntityService<QueueTypeEntity> pagedEntityService, IRepository repository, ModalService modalService, AlertService alertService, HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateQueueTypeDialogView>("Crear Queue Type");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El Queue Type fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(QueueTypeEntity? queueType)
    {
        if (queueType is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditQueueTypeDialogView>(
            "Editar Queue Type",
            new Dictionary<string, object> { ["Id"] = queueType.QueueTypeId });
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El Queue Type fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(QueueTypeEntity? queueType)
    {
        if (queueType is null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync("Eliminar Queue Type", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{queueType.QueueTypeId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El Queue Type fue eliminado correctamente.");
    }
}

// Reutiliza el formulario de crear y editar Queue Type sin cambiar el contrato del API.
public abstract partial class QueueTypeFormViewModel : CrudFormViewModel<QueueTypeEntity>
{
    protected override string BaseUrl => "api/v1/queuetypes";

    protected QueueTypeFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override QueueTypeEntity CreateEntity()
    {
        return new QueueTypeEntity { Active = true };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.TypeName))
        {
            return "Debes ingresar el nombre del Queue Type.";
        }

        if (Entity.Down && Entity.Up)
        {
            return "Un Queue Type no puede ser Down y Up al mismo tiempo.";
        }

        return null;
    }
}

public partial class CreateQueueTypeDialogViewModel : QueueTypeFormViewModel
{
    public CreateQueueTypeDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditQueueTypeDialogViewModel : QueueTypeFormViewModel
{
    public EditQueueTypeDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
