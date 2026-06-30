using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractServerPage;

public partial class FormContractServer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractServer ContractServer { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<Server>? Servers = new();
    private string BaseView = "/contractcontrol";
    private string BaseComboServers = "/api/v1/servers/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadServers();
    }

    private async Task LoadServers()
    {
        var url = IsEditControl
            ? $"{BaseComboServers}/{ContractServer.ServerId}"
            : BaseComboServers;

        var responseHttp = await _repository.GetAsync<List<Server>>(url);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Servers = responseHttp.Response;
    }

    private void ServerChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var serverId))
        {
            ContractServer.ServerId = serverId;
        }
    }
}
