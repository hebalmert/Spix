using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesGen.Register;
using Spix.HttpService;
using RegisterEntity = Spix.Domain.EntitiesGen.Register;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Register;

// Lista los consecutivos con el mismo endpoint que usa Blazor.
public partial class RegisterIndexViewModel : PagedListViewModel<RegisterEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/registers";

    public RegisterIndexViewModel(
        IPagedEntityService<RegisterEntity> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateRegisterDialogView>("Crear consecutivos");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "Los consecutivos fueron guardados correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(RegisterEntity? register)
    {
        if (register is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = register.RegisterId
        };

        var result = await _modalService.ShowAsync<EditRegisterDialogView>("Editar consecutivos", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "Los consecutivos fueron actualizados correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(RegisterEntity? register)
    {
        if (register is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar consecutivos",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{register.RegisterId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "Los consecutivos fueron eliminados correctamente.");
    }
}
