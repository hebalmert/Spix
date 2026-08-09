using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.EstratoSocialPage;

public partial class CreateEstratoSocial
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private const string BaseUrl = "/api/v1/estratossociales";
    private EstratoSocial EstratoSocial = new() { ApplyTax = true };
    private bool IsSaving;

    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsSaving = true;
        HttpResponseWrapper<object> responseHttp = await _repository.PostAsync(BaseUrl, EstratoSocial);
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
