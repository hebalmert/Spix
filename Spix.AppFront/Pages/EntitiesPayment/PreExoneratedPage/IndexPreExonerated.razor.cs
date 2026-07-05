using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesPayment.PreExoneratedPage;

public partial class IndexPreExonerated
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/preexonerateds";
    private string Filter { get; set; } = string.Empty;
    private List<PreExonerated>? PreExonerateds { get; set; }
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

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<PreExonerated>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        PreExonerateds = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", "Nueva Exoneracion" }
        };

        await _modalService.ShowAsync(typeof(CreatePreExonerated), parameters, async result =>
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
            { "Title", "Editar Exoneracion" }
        };

        await _modalService.ShowAsync(typeof(EditPreExonerated), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Eliminar",
            Text = "Desea eliminar esta exoneracion?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Eliminar",
            CancelButtonText = "Cancelar"
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Eliminado", "Registro eliminado correctamente.", SweetAlertIcon.Success);
        await LoadAsync(CurrentPage);
    }
}
