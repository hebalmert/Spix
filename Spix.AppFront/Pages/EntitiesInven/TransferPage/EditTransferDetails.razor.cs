using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

//Editar una linea del traslado.
//
//Aqui el Id es el de la LINEA, no el del traslado: el que llega del detalle. El traslado
//al que pertenece viene adentro del propio registro.
//
//El SweetAlert de exito no va aqui: lo dispara el detalle cuando el modal ya se cerro.
public partial class EditTransferDetails
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private TransferDetails? TransferDetails;

    private FormTransferDetails? FormTransferDetails { get; set; }

    private string BaseUrl = "api/v1/transferDetails";
    private bool isLoading = false;

    [Parameter] public Guid Id { get; set; }  //TransferDetailsId

    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHTTP = await _repository.GetAsync<TransferDetails>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        TransferDetails = responseHTTP.Response;
    }

    private async Task Edit()
    {
        var responseHTTP = await _repository.PutAsync($"{BaseUrl}", TransferDetails);
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
