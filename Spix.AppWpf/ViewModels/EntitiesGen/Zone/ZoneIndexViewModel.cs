using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Zone;
using Spix.HttpService;
using ZoneEntity = Spix.Domain.EntitiesGen.Zone;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Zone;

// Lista las zonas paginadas con el mismo endpoint que usa Blazor.
public partial class ZoneIndexViewModel : PagedListViewModel<ZoneEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/zones";

    public ZoneIndexViewModel(
        IPagedEntityService<ZoneEntity> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateZoneDialogView>("Crear zona");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La zona fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ZoneEntity? zone)
    {
        if (zone is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = zone.ZoneId
        };

        var result = await _modalService.ShowAsync<EditZoneDialogView>("Editar zona", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La zona fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ZoneEntity? zone)
    {
        if (zone is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar zona",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{zone.ZoneId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La zona fue eliminada correctamente.");
    }
}
