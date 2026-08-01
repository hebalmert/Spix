using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.RunSuspendedPage;

public partial class DetailsRunSuspended
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/runsuspended";
    private RunSuspended? Model { get; set; }
    private List<IntItemModel> Months { get; set; } = new();
    private bool IsLoading { get; set; }
    private bool IsRunning { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();
        IsLoading = false;
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Months = responseHttp.Response ?? new();
        }
    }

    private async Task LoadModelAsync()
    {
        var responseHttp = await _repository.GetAsync<RunSuspended>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
    }

    private async Task RunAsync()
    {
        if (Model is null)
        {
            return;
        }

        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Correr Corte",
            Text = "Se suspenderan los contratos activos que tengan una cuenta por cobrar con saldo pendiente. Desea continuar?",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Correr Corte",
            CancelButtonText = "Cancelar"
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
        {
            return;
        }

        IsRunning = true;
        var responseHttp = await _repository.PostAsync<object, RunSuspended>(
            $"{BaseUrl}/{Model.RunSuspendedId}/run",
            new { });
        IsRunning = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _sweetAlert.FireAsync(
            "Corte ejecutado",
            "El corte general fue ejecutado correctamente.",
            SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
