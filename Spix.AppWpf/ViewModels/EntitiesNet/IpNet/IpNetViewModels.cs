using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesNet.IpNet;
using IpNetEntity = Spix.Domain.EntitiesNet.IpNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesNet.IpNet;

// Lista las direcciones IP disponibles para clientes y concentra sus operaciones de pool.
public partial class IpNetIndexViewModel : PagedListViewModel<IpNetEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/ipnets";

    public IpNetIndexViewModel(
        IPagedEntityService<IpNetEntity> pagedEntityService,
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

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateIpNetDialogView>("Crear IP cliente");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La direccion IP fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(IpNetEntity? ipNet)
    {
        if (ipNet is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object> { ["Id"] = ipNet.IpNetId };
        var result = await _modalService.ShowAsync<EditIpNetDialogView>("Editar IP cliente", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La direccion IP fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(IpNetEntity? ipNet)
    {
        if (ipNet is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar IP cliente",
            "Esta accion no se puede deshacer.",
            "Eliminar");
        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"api/v1/ipnets/{ipNet.IpNetId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La direccion IP fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task CreatePoolAsync()
    {
        var result = await _modalService.ShowAsync<CreateIpNetPoolDialogView>("Nuevo pool de IP clientes");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Pool creado", "Las direcciones IP fueron creadas correctamente.");
    }

    [RelayCommand]
    private async Task DeletePoolAsync()
    {
        var result = await _modalService.ShowAsync<DeleteIpNetPoolDialogView>("Eliminar pool de IP clientes");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Pool eliminado", "Las direcciones IP fueron eliminadas correctamente.");
    }
}

// Reutiliza el guardado del formulario individual de direccion IP.
public abstract partial class IpNetFormViewModel : CrudFormViewModel<IpNetEntity>
{
    protected override string BaseUrl => "api/v1/ipnets";

    protected IpNetFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override IpNetEntity CreateEntity()
    {
        return new IpNetEntity { Active = true };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.Ip)
            ? "Debes ingresar la direccion IP."
            : null;
    }
}

public partial class CreateIpNetDialogViewModel : IpNetFormViewModel
{
    public CreateIpNetDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditIpNetDialogViewModel : IpNetFormViewModel
{
    public EditIpNetDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}

// Encapsula la creacion o eliminacion de un rango de direcciones IP.
public abstract partial class IpNetPoolDialogViewModel : ObservableObject
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private IpNetPoolCreateDTO _entity = new();

    [ObservableProperty]
    private bool _isSaving;

    protected abstract string Endpoint { get; }

    protected IpNetPoolDialogViewModel(
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Entity.IpAddress))
        {
            await _alertService.WarningAsync("Campo requerido", "Debes ingresar la base del rango IP.");
            return;
        }

        if (Entity.Desde > Entity.Hasta)
        {
            await _alertService.WarningAsync("Rango invalido", "El valor desde no puede ser mayor que hasta.");
            return;
        }

        IsSaving = true;
        try
        {
            var response = await _repository.PostAsync(Endpoint, Entity);
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

public partial class CreateIpNetPoolDialogViewModel : IpNetPoolDialogViewModel
{
    protected override string Endpoint => "api/v1/ipnets/pool";

    public CreateIpNetPoolDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }
}

public partial class DeleteIpNetPoolDialogViewModel : IpNetPoolDialogViewModel
{
    protected override string Endpoint => "api/v1/ipnets/pool/delete";

    public DeleteIpNetPoolDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }
}
