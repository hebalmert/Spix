using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.RunSuspendedPage;

public partial class EditRunSuspended
{
    [Inject] private IStringLocalizer<Resource> _localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/runsuspended";
    private RunSuspended? Model { get; set; }
    private List<IntItemModel> Months { get; set; } = new();
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();
        IsLoading = false;
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Months = responseHttp.Response ?? new();
        }
    }

    private async Task LoadModelAsync()
    {
        var responseHttp = await _repository.GetAsync<RunSuspended>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
    }

    private async Task UpdateAsync()
    {
        if (Model is null)
        {
            return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _sweetAlert.FireAsync(
            "Actualizado",
            "El corte general fue actualizado correctamente.",
            SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
