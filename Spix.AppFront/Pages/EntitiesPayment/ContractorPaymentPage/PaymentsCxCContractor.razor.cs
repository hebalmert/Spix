using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractorPaymentPage;

//Lo que se le ha pagado a la cuenta del contratista: cada abono, pagina por pagina
public partial class PaymentsCxCContractor
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/contractor-payments";
    private const int PageSize = 10;

    private CxCContractor? Model;
    private List<CxCContractorPaymentItemDto> Payments = new();
    private int CurrentPage = 1;
    private int TotalPages;
    private bool IsLoading;

    //Lo abonado sale de la cabecera, sin recorrer los abonos
    private decimal Paid => (Model?.Total ?? 0) - (Model?.Balance ?? 0);

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadModelAsync();

        if (Model is not null)
            await LoadPaymentsAsync();

        IsLoading = false;
    }

    private async Task LoadModelAsync()
    {
        var responseHttp = await _repository.GetAsync<CxCContractor>($"{BaseUrl}/cxc/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
    }

    private async Task LoadPaymentsAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<CxCContractorPaymentItemDto>>(
            $"{BaseUrl}/cxc/{Id}/payments?page={page}&recordsnumber={PageSize}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Payments = responseHttp.Response ?? new();
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadPaymentsAsync(page);
    }

    //El modo de pago, en palabras
    private string ModeName(string? mode) => mode switch
    {
        "Cash" => Localizer["Pay_Cash"],
        "Card" => Localizer["Pay_Card"],
        "Transfer" => Localizer["Pay_Transfer"],
        _ => mode ?? string.Empty
    };

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
