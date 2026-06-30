using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractNodePage;

public partial class CreateContractNode
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ContractNode ContractNode = new();

    private string BaseUrl = "/api/v1/contractnodes";
    private bool isLoading = false;
    private bool IsSaving = false;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsSaving = true;
        ContractNode.ContractClientId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", ContractNode);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
