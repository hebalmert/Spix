using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMacPage;

public partial class FormContractMac
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractMac ContractMac { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }


    private List<GuidItemModel>? ListMacs = new();
    private string BaseView = "/contractcontrol";
    private string BaseComboMacs = "/api/v1/cargueDetails/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadMacs();
    }

    private async Task LoadMacs()
    {
        var responseHttp2 = await _repository.GetAsync<List<GuidItemModel>>($"{BaseComboMacs}");
        bool errorHandler2 = await _responseHandler.HandleErrorAsync(responseHttp2);
        if (errorHandler2)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        ListMacs = responseHttp2.Response;

    }

    private void MacsChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var macid))
        {
            ContractMac.CargueDetailId = macid;
        }
    }
}