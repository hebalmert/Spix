using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.RunSuspendedPage;

public partial class IndexRunSuspended
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/runsuspended";
    private int CurrentPage = 1;
    private int TotalPages;
    private const int PageSize = 15;
    private string Filter { get; set; } = string.Empty;
    private List<RunSuspended>? Runs { get; set; }
    private List<IntItemModel> Months { get; set; } = new();

    //Los numeros del tablero: se piden una sola vez al abrir
    private CorteSummaryDto? Summary { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await LoadMonthsAsync();
        await LoadSummaryAsync();
        await LoadAsync();
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Months = responseHttp.Response ?? new();
        }
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<CorteSummaryDto>($"{BaseUrl}/summary");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Summary = responseHttp.Response;
        }
    }

    private string GetMonthName(MonthType monthType)
    {
        return Months.FirstOrDefault(x => x.Value == (int)monthType)?.Name ?? monthType.ToString();
    }

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

    private async Task ShowCreateAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", Localizer["Corte_New"].Value }
        };

        await _modalService.ShowAsync(typeof(CreateRunSuspended), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadSummaryAsync();
                await LoadAsync(CurrentPage);
            }
        });
    }

    private async Task ShowDetailsAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", Localizer["Corte_Details"].Value }
        };

        await _modalService.ShowAsync(typeof(DetailsRunSuspended), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadSummaryAsync();
                await LoadAsync(CurrentPage);
            }
        });
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", Localizer["Corte_Edit"].Value }
        };

        await _modalService.ShowAsync(typeof(EditRunSuspended), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadSummaryAsync();
                await LoadAsync(CurrentPage);
            }
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_DeleteConfirmationTitle)],
            Localizer[nameof(Resource.msg_DeleteConfirmationText)],
            SweetAlertIcon.Success);
        await LoadAsync(CurrentPage);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Uri.EscapeDataString(Filter)}";
        }

        var responseHttp = await _repository.GetAsync<List<RunSuspended>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Runs = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }
}
