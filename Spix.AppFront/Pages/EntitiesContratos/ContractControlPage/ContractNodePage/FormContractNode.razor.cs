using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractNodePage;

public partial class FormContractNode
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractNode ContractNode { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<Node>? Nodes = new();
    private string BaseView = "/contractcontrol";
    private string BaseComboNodes = "/api/v1/nodes/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadNodes();
    }

    private async Task LoadNodes()
    {
        var url = IsEditControl ? $"{BaseComboNodes}/{ContractNode.NodeId}" : BaseComboNodes;
        var responseHttp = await _repository.GetAsync<List<Node>>(url);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Nodes = responseHttp.Response;
    }

    private void NodeChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var nodeId))
        {
            ContractNode.NodeId = nodeId;
        }
    }
}
