using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.SerialPage;

public partial class EditSerialsDetails
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private CargueDetail? CargueDetail;

    private string BaseUrl = "/api/v1/cargueDetails";
    private string BaseView = "/Serials";
    private bool IsVisible = false;
    [Parameter] public Guid Id { get; set; }  //Es CargueDetailId
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsVisible = true;
        var responseHttp = await _repository.GetAsync<CargueDetail>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        IsVisible = false;
        CargueDetail = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsVisible = true;
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
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", NewModel);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        IsVisible = false;
        _navigationManager.NavigateTo($"{BaseView}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}");
    }
}