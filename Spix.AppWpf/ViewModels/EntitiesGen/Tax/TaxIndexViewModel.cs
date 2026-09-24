using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Tax;
using Spix.HttpService;
using TaxEntity = Spix.Domain.EntitiesGen.Tax;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Tax;

// Lista los impuestos paginados con el mismo endpoint que usa Blazor.
public partial class TaxIndexViewModel : PagedListViewModel<TaxEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/taxes";

    public TaxIndexViewModel(
        IPagedEntityService<TaxEntity> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateTaxDialogView>("Crear impuesto");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El impuesto fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(TaxEntity? tax)
    {
        if (tax is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = tax.TaxId
        };

        var result = await _modalService.ShowAsync<EditTaxDialogView>("Editar impuesto", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El impuesto fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(TaxEntity? tax)
    {
        if (tax is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar impuesto",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{tax.TaxId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El impuesto fue eliminado correctamente.");
    }
}
