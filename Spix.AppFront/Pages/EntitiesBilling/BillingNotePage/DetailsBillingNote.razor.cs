using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNotePage;

public partial class DetailsBillingNote
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private BillingNote? Model;
    private List<IntItemModel>? Months;
    private bool isLoading;
    private const string BaseUrl = "api/v1/billingnotes";

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();
        isLoading = false;
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
    }

    private async Task LoadModelAsync()
    {
        var responseHttp = await _repository.GetAsync<BillingNote>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
    }

    private async Task LaunchNotes()
    {
        await _sweetAlert.FireAsync("Pendiente", "El lanzamiento de notas es el siguiente paso de implementacion.", SweetAlertIcon.Info);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
