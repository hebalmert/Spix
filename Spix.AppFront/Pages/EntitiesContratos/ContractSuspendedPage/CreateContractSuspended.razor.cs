using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractSuspendedPage;

//Suspender un contrato desde el modulo. Llama al endpoint PROPIO de suspension:
//cada modulo maneja sus reglas, no se comparten con Control de Contratos.
public partial class CreateContractSuspended : IDisposable
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractsuspended";

    //Minimo de caracteres y pausa antes de consultar, igual que el autocompletar del sistema
    private const int MinLength = 3;
    private const int DelayMs = 500;

    private string SearchText = string.Empty;
    private List<ActiveContractDTO>? Candidates;
    private ActiveContractDTO? Selected;
    private string? Motivo;
    private bool IsSearching;
    private bool IsSaving;
    private CancellationTokenSource? cts;

    private async Task SearchAsync(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;
        Selected = null;

        cts?.Cancel();

        if (SearchText.Trim().Length < MinLength)
        {
            Candidates = null;
            IsSearching = false;
            return;
        }

        cts = new CancellationTokenSource();
        var token = cts.Token;

        try
        {
            await Task.Delay(DelayMs, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        IsSearching = true;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.GetAsync<List<ActiveContractDTO>>(
            $"{BaseUrl}/active?filter={Uri.EscapeDataString(SearchText.Trim())}");

        if (token.IsCancellationRequested)
            return;

        IsSearching = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Candidates = new();
            return;
        }

        Candidates = responseHttp.Response ?? new();
        await InvokeAsync(StateHasChanged);
    }

    private void Select(ActiveContractDTO item)
    {
        Selected = item;
    }

    private async Task SaveAsync()
    {
        if (Selected is null)
            return;

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Suspend_Title"],
            Text = Localizer["Suspend_Question", Selected.ControlContrato, Selected.ClientName],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Suspend_Button"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirm.IsDismissed || confirm.Value != "true")
            return;

        //Se repinta a mano: Blazor no lo hace entre los await del mismo manejador,
        //y sin esto el spinner del boton no alcanza a verse.
        IsSaving = true;
        await InvokeAsync(StateHasChanged);

        //Endpoint propio del modulo: sus reglas no son las de Control de Contratos
        var url = $"{BaseUrl}/{Selected.ContractClientId}/suspend" +
                  $"?motivo={Uri.EscapeDataString(Motivo ?? string.Empty)}";

        var responseHttp = await _repository.PostAsync(url, new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsSaving = false;
            return;
        }

        IsSaving = false;
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
