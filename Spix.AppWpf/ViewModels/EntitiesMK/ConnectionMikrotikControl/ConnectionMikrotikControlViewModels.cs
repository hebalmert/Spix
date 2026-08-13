using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesMK.ConnectionMikrotikControl;
using ConnectionMikrotikControlEntity = Spix.Domain.EntitiesMK.ConnectionMikrotikControl;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesMK.ConnectionMikrotikControl;

// Administra la configuracion de control MikroTik de la corporacion autenticada.
public partial class ConnectionMikrotikControlIndexViewModel : PagedListViewModel<ConnectionMikrotikControlEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/connectionmikrotikcontrols";

    public ConnectionMikrotikControlIndexViewModel(IPagedEntityService<ConnectionMikrotikControlEntity> pagedEntityService, IRepository repository, ModalService modalService, AlertService alertService, HttpResponseHandler responseHandler)
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
        var result = await _modalService.ShowAsync<CreateConnectionMikrotikControlDialogView>("Crear control MikroTik");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El control MikroTik fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ConnectionMikrotikControlEntity? control)
    {
        if (control is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditConnectionMikrotikControlDialogView>(
            "Editar control MikroTik",
            new Dictionary<string, object> { ["Id"] = control.ConnectionMikrotikControlId });
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El control MikroTik fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ConnectionMikrotikControlEntity? control)
    {
        if (control is null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync("Eliminar control MikroTik", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{control.ConnectionMikrotikControlId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El control MikroTik fue eliminado correctamente.");
    }
}

// Expone el enum existente como ComboBox para conservar las opciones oficiales del dominio.
public abstract partial class ConnectionMikrotikControlFormViewModel : CrudFormViewModel<ConnectionMikrotikControlEntity>
{
    [ObservableProperty]
    private ObservableCollection<MikrotikControlType> _controlTypes = new(Enum.GetValues<MikrotikControlType>());

    protected override string BaseUrl => "api/v1/connectionmikrotikcontrols";

    protected ConnectionMikrotikControlFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override ConnectionMikrotikControlEntity CreateEntity()
    {
        return new ConnectionMikrotikControlEntity
        {
            MikrotikControlType = MikrotikControlType.Ninguno
        };
    }

    protected override string? GetValidationMessage()
    {
        return null;
    }
}

public partial class CreateConnectionMikrotikControlDialogViewModel : ConnectionMikrotikControlFormViewModel
{
    public CreateConnectionMikrotikControlDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditConnectionMikrotikControlDialogViewModel : ConnectionMikrotikControlFormViewModel
{
    public EditConnectionMikrotikControlDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
