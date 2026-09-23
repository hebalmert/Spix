using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.CxCBillPage;

//Explica una cuenta por cobrar: de que se compone (plan y servicios) y como se cruzo
//con el pago adelantado y la exoneracion hasta quedar en su saldo.
public partial class DetailsCxCBill
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/cxcbills";

    private CxCBill? Model;
    private List<IntItemModel> Months = new();
    private bool isLoading;

    //Lo que se aplico: el adelanto como pago y la exoneracion como descuento
    private decimal Payment => Model?.CxCBillDetails?.Sum(x => x.Payment) ?? 0;

    private decimal Discount => Model?.CxCBillDetails?.Sum(x => x.Discount) ?? 0;

    private string MonthName => Months.FirstOrDefault(x => x.Value == (int)(Model?.MonthType ?? 0))?.Name
                                ?? Model?.MonthType.ToString() ?? string.Empty;

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();
        isLoading = false;
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
    }

    private async Task LoadModelAsync()
    {
        var responseHttp = await _repository.GetAsync<CxCBill>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
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
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
