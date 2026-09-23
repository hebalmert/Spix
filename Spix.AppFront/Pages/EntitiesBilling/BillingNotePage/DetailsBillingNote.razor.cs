using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNotePage;

public partial class DetailsBillingNote
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private BillingNote? Model;
    private List<IntItemModel>? Months;
    //Lo que devuelve la revision previa
    private List<BillingCheckDto>? Checks;

    private List<BillingCheckDto> Incomplete => Checks is null
        ? new()
        : Checks.Where(x => !x.AlreadyBilled && Missing(x).Count > 0).ToList();

    private bool isLoading;

    //El avance del lanzamiento por lotes
    private bool IsLaunching;
    private int ToProcess;
    private int Processed;

    private int Percent => ToProcess == 0 ? 0 : Math.Min(100, Processed * 100 / ToProcess);

    private const string BaseUrl = "api/v1/billingnotes";

    //Cuantos contratos van por request. Lotes chicos = avance fino y nada de transacciones largas.
    private const int BatchSize = 25;

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

    //Un solo boton: primero revisa, avisa si algo bloquea, y despues factura por lotes
    //mostrando el avance. Cada lote se confirma solo: si se cae la red, lo hecho queda hecho
    //y al volver a lanzar se salta lo ya facturado.
    private async Task LaunchNotes()
    {
        if (IsLaunching)
            return;

        //Paso 1: la revision
        IsLaunching = true;
        Processed = 0;
        ToProcess = 0;

        var check = await _repository.GetAsync<List<BillingCheckDto>>($"{BaseUrl}/{Id}/check");
        if (await _responseHandler.HandleErrorAsync(check))
        {
            IsLaunching = false;
            return;
        }

        Checks = check.Response ?? new();
        StateHasChanged();

        //Los que se van a facturar: activos, sin nota del periodo y COMPLETOS
        var pendientes = Checks
            .Where(x => !x.AlreadyBilled && Missing(x).Count == 0)
            .Select(x => x.ContractClientId)
            .ToList();

        var bloqueados = Checks.Count(x => !x.AlreadyBilled && Missing(x).Count > 0);

        if (pendientes.Count == 0)
        {
            IsLaunching = false;
            await _sweetAlert.FireAsync(Localizer["Billing_Launch"], Localizer["Billing_NothingToDo"], SweetAlertIcon.Info);
            return;
        }

        //Paso 2: confirmar, avisando lo que queda fuera
        var texto = Localizer["Billing_ConfirmText", pendientes.Count].Value;
        if (bloqueados > 0)
            texto += " " + Localizer["Billing_ConfirmBlocked", bloqueados].Value;

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Billing_Launch"],
            Text = texto,
            Icon = bloqueados > 0 ? SweetAlertIcon.Warning : SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Billing_Launch"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirm.IsDismissed || confirm.Value != "true")
        {
            IsLaunching = false;
            return;
        }

        //Paso 3: lote por lote, moviendo la barra
        ToProcess = pendientes.Count;
        var creadas = 0;
        var saltadas = 0;
        var fuera = new List<BillingLaunchIssueDto>();

        for (var i = 0; i < pendientes.Count; i += BatchSize)
        {
            var lote = pendientes.Skip(i).Take(BatchSize).ToList();
            var responseHttp = await _repository.PostAsync<List<Guid>, BillingLaunchResultDto>($"{BaseUrl}/{Id}/launch/batch", lote);

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                //Se corta aqui: lo ya facturado quedo guardado y la nota sigue abierta
                IsLaunching = false;
                await MostrarResumenAsync(creadas, saltadas, fuera, false);
                return;
            }

            var parcial = responseHttp.Response ?? new BillingLaunchResultDto();
            creadas += parcial.Created;
            saltadas += parcial.Skipped;
            fuera.AddRange(parcial.Issues);

            Processed += lote.Count;
            StateHasChanged();
        }

        //Paso 4: cerrar la nota
        var finish = await _repository.PostAsync($"{BaseUrl}/{Id}/launch/finish", new { });
        IsLaunching = false;

        if (await _responseHandler.HandleErrorAsync(finish))
            return;

        await MostrarResumenAsync(creadas, saltadas, fuera, true);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    //El resumen de lo que paso
    private async Task MostrarResumenAsync(int creadas, int saltadas, List<BillingLaunchIssueDto> fuera, bool completo)
    {
        var texto = $"{Localizer["Billing_ResultCreated", creadas]}\n{Localizer["Billing_ResultSkipped", saltadas]}";

        if (fuera.Count > 0)
        {
            texto += "\n" + Localizer["Billing_ResultIssues", fuera.Count] + "\n" +
                     string.Join("\n", fuera.Take(10).Select(x => $"#{x.ControlContrato} {x.ClientFullName}: {x.Reason}"));
        }

        if (!completo)
            texto += "\n" + Localizer["Billing_Interrupted"];

        await _sweetAlert.FireAsync(
            Localizer[completo ? "Billing_Launched" : "Billing_Interrupted"],
            texto,
            fuera.Count > 0 || !completo ? SweetAlertIcon.Warning : SweetAlertIcon.Success);
    }

    //Un contrato ACTIVO debe estar completo: plan, IP, MAC, servidor, nodo, queue e ipbinding.
    //Si le falta cualquiera, no se le lanza nota hasta que lo completen.
    private static List<(string Name, bool Blocks)> Missing(BillingCheckDto item)
    {
        var faltas = new List<(string, bool)>();

        if (!item.HasPlan) faltas.Add(("Plan", true));
        if (!item.HasIp) faltas.Add(("IP", true));
        if (!item.HasMac) faltas.Add(("MAC", true));
        if (!item.HasServer) faltas.Add(("Servidor", true));
        if (!item.HasNode) faltas.Add(("Nodo", true));
        if (!item.HasQueue) faltas.Add(("Queue", true));
        if (!item.HasBinding) faltas.Add(("IpBinding", true));

        return faltas;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
