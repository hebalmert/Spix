using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesBilling.MyBillPage;

//El detalle de una factura del cliente. Se pide al abrir el modal: asi el listado
//viaja liviano y solo se cargan los renglones de la factura que quiso ver.
public partial class DetailsMyBill
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService ModalService { get; set; } = null!;

    private const string BaseUrl = "api/v1/mybills";

    [Parameter, EditorRequired] public Guid SellId { get; set; }

    private MyBillDetailDto? Bill;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<MyBillDetailDto>($"{BaseUrl}/{SellId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await Return();
            return;
        }

        Bill = responseHttp.Response;
        await InvokeAsync(StateHasChanged);
    }

    //De donde sale el renglon, en palabras
    private string OriginName(string? origin) => origin switch
    {
        "Plan" => Localizer[nameof(Resource.Plan)],
        "SolicitudServicio" => Localizer["Sell_OriginService"],
        _ => origin ?? string.Empty
    };

    private async Task Return()
    {
        await ModalService.CloseAsync(ModalResult.Cancel());
    }
}
