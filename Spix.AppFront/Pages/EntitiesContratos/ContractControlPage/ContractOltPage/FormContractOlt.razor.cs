using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractOltPage;

public partial class FormContractOlt
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractOlt ContractOlt { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<Olt>? Olts = new();
    private string BaseView = "/contractcontrol";
    private string BaseComboOlts = "/api/v1/olts/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadOlts();
    }

    private async Task LoadOlts()
    {
        var url = IsEditControl ? $"{BaseComboOlts}/{ContractOlt.OltId}" : BaseComboOlts;
        var responseHttp = await _repository.GetAsync<List<Olt>>(url);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Olts = responseHttp.Response;
    }

    private void OltChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var oltId))
        {
            ContractOlt.OltId = oltId;
        }
    }
}
