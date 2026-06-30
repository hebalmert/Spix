using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesMK.ConnectionMikrotikControlPage;

public partial class CreateConnectionMikrotikControl
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ConnectionMikrotikControl ConnectionMikrotikControl = new() { MikrotikControlType = MikrotikControlType.Ninguno };
    private string BaseUrl = "/api/v1/connectionmikrotikcontrols";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, ConnectionMikrotikControl);
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
