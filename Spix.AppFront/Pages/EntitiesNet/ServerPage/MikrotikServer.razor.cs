using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesNet.ServerPage;

public partial class MikrotikServer
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private MkConnectionResultDTO? Result;
    private bool IsLoading = true;

    private string BaseUrl = "api/v1/mkconnections/mkchecks";

    protected override async Task OnInitializedAsync()
    {
        await LoadPing();
    }

    private async Task LoadPing()
    {
        IsLoading = true;

        var responseHttp = await _repository.GetAsync<MkConnectionResultDTO>($"{BaseUrl}/{Id}");
        IsLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsLoading = false;
            return;
        }

        Result = responseHttp.Response;
        
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}