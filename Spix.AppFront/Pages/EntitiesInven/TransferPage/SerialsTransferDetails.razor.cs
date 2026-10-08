using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

//Los equipos que viajaron en una linea del traslado. Solo lee: no se edita nada aqui.
public partial class SerialsTransferDetails
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private const string BaseUrl = "api/v1/transferDetails";

    private List<GuidItemModel>? Serials;

    [Parameter, EditorRequired] public Guid TransferDetailsId { get; set; }

    [Parameter] public string? ProductName { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHTTP = await _repository.GetAsync<List<GuidItemModel>>(
            $"{BaseUrl}/serials/moved/{TransferDetailsId}");

        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            //Lista vacia y no nula: con null el GenericList se queda girando para siempre
            Serials = new List<GuidItemModel>();
            return;
        }

        Serials = responseHTTP.Response ?? new List<GuidItemModel>();
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
