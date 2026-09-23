using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.PrePaymentPage;

//Pagos recibidos por adelantado que aun no se cruzan con una nota de cobro
public partial class IndexPrePayment
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private const string BaseUrl = "api/v1/prepayments";

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private string Filter { get; set; } = string.Empty;

    private List<PrePayment>? PrePayments { get; set; }
    private PrePaymentSummaryDto? Summary;
    private List<IntItemModel> Months { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMonthsAsync();
            await ReloadAsync();
        }
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
    }

    //El tablero se cuenta sobre todo lo pendiente, no solo la pagina visible
    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<PrePaymentSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Summary = responseHttp.Response;
    }

    //Despues de crear, editar o borrar: tabla y tablero. Paginar y buscar solo recargan la tabla.
    private async Task ReloadAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage);
    }

    private string GetMonthName(MonthType monthType) =>
        Months.FirstOrDefault(x => x.Value == (int)monthType)?.Name ?? monthType.ToString();

    //Cuantos servicios tecnicos trae el adelanto, ademas del plan
    private static int ServiceLines(PrePayment item) =>
        item.PrePaymentDetails?.Count(x => x.ServiceRequestDetailId.HasValue) ?? 0;

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<PrePayment>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        PrePayments = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", $"{Localizer["PrePayment_New"]}" }
        };

        await _modalService.ShowAsync(typeof(CreatePrePayment), parameters, async result =>
        {
            if (result.Succeeded)
                await ReloadAsync();
        });
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", $"{Localizer["PrePayment_Edit"]}" }
        };

        await _modalService.ShowAsync(typeof(EditPrePayment), parameters, async result =>
        {
            if (result.Succeeded)
                await ReloadAsync();
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer["PrePayment_DeleteMessage"],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await ReloadAsync();
    }
}
