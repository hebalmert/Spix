using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

//Editar el encabezado del traslado. Faltaba: el index abria el modal de Proveedores.
//
//El SweetAlert de exito NO va aqui: lo dispara el index cuando el modal ya se cerro,
//igual que en Compras y en el resto del sistema.
public partial class EditTransfer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private string BaseUrl = "api/v1/transfers";
    private Transfer? Transfer;
    private bool isLoading = false;

    private FormTransfer? FormTransfer { get; set; }

    [Parameter] public Guid Id { get; set; }

    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHTTP = await _repository.GetAsync<Transfer>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Transfer = responseHTTP.Response;
    }

    private async Task Edit()
    {
        var responseHTTP = await _repository.PutAsync($"{BaseUrl}", Transfer);
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
