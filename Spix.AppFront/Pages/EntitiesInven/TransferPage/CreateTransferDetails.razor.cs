using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

public partial class CreateTransferDetails
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    private TransferDetails TransferDetails = new();

    private FormTransferDetails? FormTransferDetails { get; set; }
    private bool isLoading = false;
    [Parameter] public string? Title { get; set; }
    [Parameter] public Guid Id { get; set; }  //TransferId

    private string BaseUrl = "/api/v1/transferDetails";
    private string BaseView = "/transfers/details";

    protected override void OnInitialized()
    {
        TransferDetails.TransferId = Id;
    }

    private async Task Create()
    {
        isLoading = true;
        var responseHttp = await _repository.PostAsync<TransferDetails, TransferDetails>($"{BaseUrl}", TransferDetails);
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        //Los equipos elegidos se guardan DESPUES, porque hasta ahora no existia la linea
        //a la cual reservarlos.
        var creada = responseHttp.Response;
        if (creada is not null && FormTransferDetails?.SelectedSerials.Count > 0)
        {
            var seriales = await _repository.PostAsync($"{BaseUrl}/serials/{creada.TransferDetailsId}",
                FormTransferDetails.SelectedSerials.ToList());

            if (await _responseHandler.HandleErrorAsync(seriales))
            {
                await _modalService.CloseAsync(ModalResult.Ok());
                return;
            }
        }
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}