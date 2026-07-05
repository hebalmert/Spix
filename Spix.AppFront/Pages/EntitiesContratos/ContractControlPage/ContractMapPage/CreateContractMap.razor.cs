using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMapPage;

public partial class CreateContractMap
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public ContractMap? Model { get; set; }
    [Parameter] public string? Title { get; set; }

    private ContractMap ContractMap = new();
    private bool IsSaving;
    private const string BaseUrl = "/api/v1/contractmaps";

    protected override void OnInitialized()
    {
        if (Model is not null && Model.ContractMapId != Guid.Empty)
        {
            ContractMap = new ContractMap
            {
                ContractMapId = Model.ContractMapId,
                ContractClientId = Model.ContractClientId,
                Latitude = Model.Latitude,
                Longitude = Model.Longitude
            };
        }
        else
        {
            ContractMap.ContractClientId = Id;
        }
    }

    private async Task Save()
    {
        IsSaving = true;
        var responseHttp = ContractMap.ContractMapId == Guid.Empty
            ? await _repository.PostAsync(BaseUrl, ContractMap)
            : await _repository.PutAsync(BaseUrl, ContractMap);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
