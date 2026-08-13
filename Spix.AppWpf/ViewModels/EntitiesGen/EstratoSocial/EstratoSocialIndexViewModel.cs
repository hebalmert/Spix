using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.EstratoSocial;
using Spix.HttpService;
using EstratoSocialEntity = Spix.Domain.EntitiesGen.EstratoSocial;

namespace Spix.AppWpf.ViewModels.EntitiesGen.EstratoSocial;

// Consulta los estratos sociales paginados sin descargar todos los registros de la corporacion.
public partial class EstratoSocialIndexViewModel : PagedListViewModel<EstratoSocialEntity>
{
    protected override string Endpoint => "api/v1/estratossociales";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    public EstratoSocialIndexViewModel(
        IPagedEntityService<EstratoSocialEntity> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateEstratoSocialDialogView>("Crear estrato social");
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El estrato social fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(EstratoSocialEntity? entity)
    {
        if (entity is null) return;
        var result = await _modalService.ShowAsync<EditEstratoSocialDialogView>("Editar estrato social", new Dictionary<string, object> { ["Id"] = entity.EstratoSocialId });
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El estrato social fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(EstratoSocialEntity? entity)
    {
        if (entity is null) return;
        var confirmed = await _alertService.ConfirmAsync("Eliminar estrato social", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed) return;
        var responseHttp = await _repository.DeleteAsync($"{Endpoint}/{entity.EstratoSocialId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El estrato social fue eliminado correctamente.");
    }
}
