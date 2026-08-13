using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesNet.IpNetwork;
using IpNetworkEntity = Spix.Domain.EntitiesNet.IpNetwork;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;

// Lista las direcciones IP de red y administra sus operaciones de pool.
public partial class IpNetworkIndexViewModel : PagedListViewModel<IpNetworkEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/ipnetworks";

    public IpNetworkIndexViewModel(
        IPagedEntityService<IpNetworkEntity> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateIpNetworkDialogView>("Crear IP red");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La direccion IP de red fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(IpNetworkEntity? ipNetwork)
    {
        if (ipNetwork is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object> { ["Id"] = ipNetwork.IpNetworkId };
        var result = await _modalService.ShowAsync<EditIpNetworkDialogView>("Editar IP red", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La direccion IP de red fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(IpNetworkEntity? ipNetwork)
    {
        if (ipNetwork is null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync("Eliminar IP red", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{ipNetwork.IpNetworkId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La direccion IP de red fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task CreatePoolAsync()
    {
        var result = await _modalService.ShowAsync<CreateIpNetworkPoolDialogView>("Nuevo pool de IP red");
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
        var result = await _modalService.ShowAsync<DeleteIpNetworkPoolDialogView>("Eliminar pool de IP red");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Pool eliminado", "Las direcciones IP fueron eliminadas correctamente.");
    }
}

// Comparte carga y guardado de una direccion IP interna.
public abstract partial class IpNetworkFormViewModel : CrudFormViewModel<IpNetworkEntity>
{
    protected override string BaseUrl => "api/v1/ipnetworks";

    protected IpNetworkFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override IpNetworkEntity CreateEntity()
    {
        return new IpNetworkEntity { Active = true };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.Ip)
            ? "Debes ingresar la direccion IP."
            : null;
    }
}

public partial class CreateIpNetworkDialogViewModel : IpNetworkFormViewModel
{
    public CreateIpNetworkDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditIpNetworkDialogViewModel : IpNetworkFormViewModel
{
    public EditIpNetworkDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}

// Gestiona la solicitud de creacion o eliminacion de un rango de IP de red.
public abstract partial class IpNetworkPoolDialogViewModel : ObservableObject
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

    protected IpNetworkPoolDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
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

public partial class CreateIpNetworkPoolDialogViewModel : IpNetworkPoolDialogViewModel
{
    protected override string Endpoint => "api/v1/ipnetworks/pool";

    public CreateIpNetworkPoolDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }
}

public partial class DeleteIpNetworkPoolDialogViewModel : IpNetworkPoolDialogViewModel
{
    protected override string Endpoint => "api/v1/ipnetworks/pool/delete";

    public DeleteIpNetworkPoolDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }
}
