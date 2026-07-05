using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class CreateServiceRequestPic
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

    protected override Task OnInitializedAsync()
    {
        Model = new() { ServiceRequestId = Id };
        IsLoading = false;
        return Task.CompletedTask;
    }

    private async Task Create()
    {
        IsSaving = true;
        Model.ServiceRequestId = Id;
        var responseHttp = await _repository.PostAsync(BaseUrl, Model);
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
