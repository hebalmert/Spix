using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.ShareLoading;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.CarguePage;

public partial class EditCargueDetails
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private CargueDetail? CargueDetail;

    private string BaseUrl = "/api/v1/cargueDetails";
    private string BaseView = "/cargues/details";
    private bool isLoading = false;
    [Parameter] public Guid Id { get; set; }  //Es CargueDetailId
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.GetAsync<CargueDetail>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        CargueDetail = responseHttp.Response;
    }

    private async Task Edit()
    {
        CargueDetail NewModel = new()
        {
            CargueDetailId = CargueDetail!.CargueDetailId,
            CargueId = CargueDetail.CargueId,
            MacWlan = CargueDetail.MacWlan,
            DateCargue = CargueDetail.DateCargue,
            Comment = CargueDetail.Comment,
            Status = CargueDetail.Status,
            CorporationId = CargueDetail.CorporationId
        };
        isLoading = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", NewModel);
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}