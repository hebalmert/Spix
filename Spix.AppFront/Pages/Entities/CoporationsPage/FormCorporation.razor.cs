using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.Entities.CoporationsPage;

public partial class FormCorporation
{
    //Services

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    //Parameters

    [Parameter, EditorRequired] public Corporation Corporation { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }

    //Local State

    private Country? SelectedCountry;
    private List<Country>? Countries;
    private SoftPlan? SelectedSoftplan;
    private SoftPlan? SoftplanDays;
    private List<SoftPlan>? SoftPlans;
    private DateTime? DateMin = new DateTime(2025, 1, 1);
    private string? ImageUrl;
    private string BaseView = "/corporations";
    private string BaseComboSoftplan = "/api/v1/softplans/loadCombo";
    private string BaseComboCountry = "/api/v1/countries/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadSoftPlan();
        await LoadCountry();
        if (IsEditControl)
        {
            ImageUrl = Corporation.ImageFullPath;
        }
        else
        {
            Corporation.DateStart = DateTime.Now;
            Corporation.DateEnd = DateTime.Now;
        }
    }

    private async Task LoadSoftPlan()
    {
        var responseHttp = await _repository.GetAsync<List<SoftPlan>>($"{BaseComboSoftplan}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        SoftPlans = responseHttp.Response;
        if (IsEditControl)
        {
            SelectedSoftplan = SoftPlans!.Where(x => x.SoftPlanId == Corporation.SoftPlanId)
                .Select(x => new SoftPlan { SoftPlanId = x.SoftPlanId, Name = x.Name })
                .FirstOrDefault();
            SoftplanDays = SoftPlans!.FirstOrDefault(x => x.SoftPlanId == Corporation.SoftPlanId);
        }
    }

    private async Task LoadCountry()
    {
        var responseHttp = await _repository.GetAsync<List<Country>>($"{BaseComboCountry}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Countries = responseHttp.Response;
        if (IsEditControl)
        {
            SelectedCountry = Countries!.Where(x => x.CountryId == Corporation.CountryId)
                .Select(x => new Country { CountryId = x.CountryId, Name = x.Name })
                .FirstOrDefault();
        }
    }

    private void ImageSelected(string imagenBase64)
    {
        Corporation.ImgBase64 = imagenBase64;
        ImageUrl = null;
    }

    private void CountryChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int selectedId))
        {
            Corporation.CountryId = selectedId;
            SelectedCountry = Countries!.FirstOrDefault(x => x.CountryId == selectedId);
        }
    }

    private void SoftPlanChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int selectedId))
        {
            Corporation.SoftPlanId = selectedId;
            SelectedSoftplan = SoftPlans!.FirstOrDefault(x => x.SoftPlanId == selectedId);

            if (SelectedSoftplan != null)
            {
                Corporation.DateStart = DateTime.Today;
                Corporation.DateEnd = Corporation.DateStart.AddMonths(SelectedSoftplan.Meses);
            }
        }
    }
}