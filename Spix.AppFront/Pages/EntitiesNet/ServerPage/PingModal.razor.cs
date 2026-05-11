using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using Spix.xNetwork.PingHelper;

namespace Spix.AppFront.Pages.EntitiesNet.ServerPage;

public partial class PingModal
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string Host { get; set; } = string.Empty;
    [Parameter] public string? Title { get; set; }

    private PingResult? Result;
    private bool IsLoading = true;

    private string BaseUrl = "api/v1/checksnet/ping";

    protected override async Task OnInitializedAsync()
    {
        await LoadPing();
    }

    private async Task LoadPing()
    {
        IsLoading = true;

        var responseHttp = await _repository.GetAsync<ActionResponse<PingResult>>($"{BaseUrl}/{Host}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsLoading = false;
            return;
        }

        Result = responseHttp.Response!.Result;
        IsLoading = false;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}