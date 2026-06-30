using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractBindPage;

public partial class FormContractBind
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractBind ContractBind { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<IntItemModel>? HotSpotTypes;
    private string BaseComboHotSpotTypes = "/api/v1/combosData/ComboHotspot";

    protected override async Task OnInitializedAsync()
    {
        await LoadHotSpotTypes();

        if (ContractBind.HotSpotTypeId == 0 && HotSpotTypes?.Count > 0)
        {
            ContractBind.HotSpotTypeId = HotSpotTypes[0].Value;
        }
    }

    private async Task LoadHotSpotTypes()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>(BaseComboHotSpotTypes);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            HotSpotTypes = new();
            return;
        }

        HotSpotTypes = responseHttp.Response;
    }

    private void HotSpotTypeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var hotSpotTypeId))
        {
            ContractBind.HotSpotTypeId = hotSpotTypeId;
        }
    }
}
