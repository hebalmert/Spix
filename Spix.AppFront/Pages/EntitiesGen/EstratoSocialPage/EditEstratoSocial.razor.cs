using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.EstratoSocialPage;

public partial class EditEstratoSocial
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private const string BaseUrl = "/api/v1/estratossociales";
    private EstratoSocial? EstratoSocial;
    private bool IsLoading;
    private bool IsSaving;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        StateHasChanged();

        HttpResponseWrapper<EstratoSocial> responseHttp = await _repository.GetAsync<EstratoSocial>($"{BaseUrl}/{Id}");
        IsLoading = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        EstratoSocial = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        HttpResponseWrapper<object> responseHttp = await _repository.PutAsync(BaseUrl, EstratoSocial);
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
