using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesNet.IpNetworkPage;

public partial class DeleteIpNetworkPool
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "/api/v1/ipnetworks";
    private IpNetPoolCreateDTO Model { get; set; } = new();
    private bool IsSaving { get; set; }

    private async Task DeletePoolAsync()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/pool/delete", Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
