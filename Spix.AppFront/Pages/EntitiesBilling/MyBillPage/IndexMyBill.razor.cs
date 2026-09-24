using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesBilling.MyBillPage;

//El portal del cliente: sus facturas y cuanto debe. El backend ya limita lo que puede
//ver a sus propias facturas, aqui solo se pinta.
public partial class IndexMyBill
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private const string BaseUrl = "api/v1/mybills";
    private const int PageSize = 15;

    private List<MyBillItemDto>? Bills;
    private MyBillSummaryDto? Summary;

    private int CurrentPage = 1;
    private int TotalPages;

    protected override async Task OnInitializedAsync()
    {
        //Los dos numeros de arriba se piden una sola vez
        var resumen = await _repository.GetAsync<MyBillSummaryDto>($"{BaseUrl}/summary");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var responseHttp = await _repository.GetAsync<List<MyBillItemDto>>($"{BaseUrl}?page={CurrentPage}&recordsnumber={PageSize}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Bills = responseHttp.Response ?? new();
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync();
    }

    //El detalle se pide solo cuando el cliente abre la factura
    private async Task ShowDetailsAsync(Guid sellId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "SellId", sellId }
        };

        await _modalService.ShowAsync(typeof(DetailsMyBill), parameters);
    }
}
