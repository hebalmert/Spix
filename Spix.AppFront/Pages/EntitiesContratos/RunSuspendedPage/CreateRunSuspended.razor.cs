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

public partial class CreateRunSuspended
{
    [Inject] private IStringLocalizer<Resource> _localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/runsuspended";
    private RunSuspended Model { get; set; } = new();
    private List<IntItemModel> Months { get; set; } = new();
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var today = DateTime.Today;
        Model.YearNumber = today.Year;
        Model.MonthType = (MonthType)today.Month;
        await LoadMonthsAsync();
    }

    private async Task LoadMonthsAsync()
    {
        IsLoading = true;
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        IsLoading = false;

        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Months = responseHttp.Response ?? new();
        }
    }

    private async Task CreateAsync()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _sweetAlert.FireAsync(
            _localizer[nameof(Resource.msg_CreateSuccessTitle)],
            _localizer[nameof(Resource.msg_CreateSuccessMessage)],
            SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
