using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class EditServiceRequestPic
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public bool IsCompleted { get; set; }

    private const string BaseUrl = "/api/v1/servicerequestpics";
    private ServiceRequestPic Model = new();
    private bool IsLoading = true;
    private bool IsSaving = false;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceRequestPic>($"{BaseUrl}/{Id}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Model = responseHttp.Response ?? new();
        }

        IsLoading = false;
    }

    private async Task Update()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, Model);
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
