using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesEmails.EmailProvider;
using Spix.Domain.EntitiesEmails;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesEmails.EmailProvider;

// Lista las configuraciones de correo, con el mismo endpoint y la misma prueba de envio
// que tiene Blazor.
public partial class EmailProviderIndexViewModel : PagedListViewModel<EmailProviderSetting>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/emailproviders";

    public EmailProviderIndexViewModel(
        IPagedEntityService<EmailProviderSetting> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateEmailProviderDialogView>("Crear configuracion de correo");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La configuracion fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(EmailProviderSetting? provider)
    {
        if (provider is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = provider.EmailProviderSettingId
        };

        var result = await _modalService.ShowAsync<EditEmailProviderDialogView>("Editar configuracion de correo", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La configuracion fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(EmailProviderSetting? provider)
    {
        if (provider is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar configuracion",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{provider.EmailProviderSettingId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La configuracion fue eliminada correctamente.");
    }

    // Manda un correo de prueba con esa configuracion, igual que el boton de Blazor
    [RelayCommand]
    private async Task TestAsync(EmailProviderSetting? provider)
    {
        if (provider is null)
        {
            return;
        }

        var response = await _repository.PostAsync($"{Endpoint}/{provider.EmailProviderSettingId}/email-test", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await _alertService.SuccessAsync("Correo de prueba", "El correo de prueba fue enviado correctamente.");
    }
}
