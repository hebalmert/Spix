using Microsoft.AspNetCore.Components;
using CurrieTechnologies.Razor.SweetAlert2;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using Microsoft.Extensions.Localization;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNoteOnePage;

public partial class IndexBillingNoteOne
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/billingnoteones";
    private string Filter { get; set; } = string.Empty;
    private List<BillingNoteOne>? BillingNoteOnes { get; set; }
    private List<IntItemModel> Months { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMonthsAsync();
            await LoadAsync();
        }
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
    }

    private string GetMonthName(MonthType monthType) =>
        Months.FirstOrDefault(x => x.Value == (int)monthType)?.Name ?? monthType.ToString();

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task ShowModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", "Nueva Nota Individual" }
        };

        await _modalService.ShowAsync(typeof(CreateBillingNoteOne), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task ShowDetailsAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Detalle Nota Cobro Cliente" }
        };

        await _modalService.ShowAsync(typeof(DetailsBillingNoteOne), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Editar Nota Cobro" }
        };

        await _modalService.ShowAsync(typeof(EditBillingNoteOne), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
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
        await LoadAsync(CurrentPage);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<BillingNoteOne>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        BillingNoteOnes = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }
}
