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
    [Inject] private IStringLocalizer<Resource> _localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/runsuspended";
    private int CurrentPage = 1;
    private int TotalPages;
    private const int PageSize = 15;
    private string Filter { get; set; } = string.Empty;
    private List<RunSuspended>? Runs { get; set; }
    private List<IntItemModel> Months { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await LoadMonthsAsync();
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
            { "Title", "Nuevo Corte General" }
        };

        await _modalService.ShowAsync(typeof(CreateRunSuspended), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadAsync(CurrentPage);
            }
        });
    }

    private async Task ShowDetailsAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Detalle Corte General" }
        };

        await _modalService.ShowAsync(typeof(DetailsRunSuspended), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadAsync(CurrentPage);
            }
        });
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Editar Corte General" }
        };

        await _modalService.ShowAsync(typeof(EditRunSuspended), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadAsync(CurrentPage);
            }
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = _localizer[nameof(Resource.msg_DeleteTitle)],
            Text = _localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = _localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = _localizer[nameof(Resource.ButtonCancel)]
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
            _localizer[nameof(Resource.msg_DeleteConfirmationTitle)],
            _localizer[nameof(Resource.msg_DeleteConfirmationText)],
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
