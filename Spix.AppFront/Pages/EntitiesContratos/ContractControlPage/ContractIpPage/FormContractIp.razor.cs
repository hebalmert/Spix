using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractIpPage;

public partial class FormContractIp
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractIp ContractIp { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }


    private List<IpNet>? IpNets = new();
    private string BaseView = "/contractcontrol";
    private string BaseComboIpNetwork = "/api/v1/ipnets/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadIpNetwork();
    }

    private async Task LoadIpNetwork()
    {
        if (IsEditControl)
        {
            var responseHttp2 = await _repository.GetAsync<List<IpNet>>($"{BaseComboIpNetwork}/{ContractIp.IpNet!.IpNetId}");
            bool errorHandler2 = await _responseHandler.HandleErrorAsync(responseHttp2);
            if (errorHandler2)
            {
                _navigationManager.NavigateTo($"{BaseView}");
                return;
            }
            IpNets = responseHttp2.Response;
        }
        else
        {

            var responseHttp = await _repository.GetAsync<List<IpNet>>($"{BaseComboIpNetwork}");
            bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
            if (errorHandler)
            {
                _navigationManager.NavigateTo($"{BaseView}");
                return;
            }
            IpNets = responseHttp.Response;
        }
    }

    private void IpNetworkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var ipnetworkId))
        {
            ContractIp.IpNetId = ipnetworkId;
        }
    }
}