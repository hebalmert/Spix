using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.RunSuspendedPage;

//El corte general: primero se revisa a quien se le va a cortar, y despues se corta
//equipo por equipo, abriendo una sola conexion por servidor.
public partial class DetailsRunSuspended
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/runsuspended";

    private RunSuspended? Model { get; set; }
    private List<IntItemModel> Months { get; set; } = new();
    private CorteCheckDto? Check { get; set; }
    private bool IsLoading { get; set; }

    //Lo que quedo cortado se mira pagina por pagina
    private List<CorteDetailDto> Details { get; set; } = new();
    private int CurrentPage = 1;
    private int TotalPages;
    private const int PageSize = 15;

    //El avance del corte, equipo por equipo
    private bool IsRunning;
    private int ToProcess;
    private int Processed;
    private string? CurrentServer;

    private int Percent => ToProcess == 0 ? 0 : Math.Min(100, Processed * 100 / ToProcess);

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();

        //Si todavia no se ha corrido se muestran los numeros; si ya corrio, lo que corto
        if (Model is not null && !Model.Executed)
        {
            await LoadCheckAsync();
        }
        else if (Model is not null)
        {
            await LoadDetailsAsync();
        }

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

    private async Task LoadCheckAsync()
    {
        var responseHttp = await _repository.GetAsync<CorteCheckDto>($"{BaseUrl}/{Id}/check");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Check = responseHttp.Response;
        }
    }

    private async Task LoadDetailsAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<CorteDetailDto>>(
            $"{BaseUrl}/{Id}/details?page={page}&recordsnumber={PageSize}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        Details = responseHttp.Response ?? new();
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadDetailsAsync(page);
    }

    //Un solo boton: revisa, confirma y corta equipo por equipo mostrando el avance.
    //Cada equipo se confirma solo: si uno no responde, lo cortado queda cortado y el
    //corte sigue abierto para continuarlo.
    private async Task RunAsync()
    {
        if (Model is null || IsRunning)
        {
            return;
        }

        IsRunning = true;
        Processed = 0;
        ToProcess = 0;
        CurrentServer = null;

        //Paso 1: la revision, por si alguien pago despues de abrir la pantalla
        await LoadCheckAsync();
        StateHasChanged();

        if (Check is null)
        {
            IsRunning = false;
            return;
        }

        if (Check.ToSuspend == 0)
        {
            IsRunning = false;
            await _sweetAlert.FireAsync(Localizer["Corte_Run"], Localizer["Corte_NothingToDo"], SweetAlertIcon.Info);
            return;
        }

        //Paso 2: confirmar, avisando los que quedan fuera por no tener IpBinding
        var sinEquipo = Check.NoServer;
        var texto = Localizer["Corte_ConfirmText", Check.ToSuspend].Value;
        if (sinEquipo > 0)
        {
            texto += " " + Localizer["Corte_ConfirmBlocked", sinEquipo].Value;
        }

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Corte_Run"],
            Text = texto,
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Corte_Run"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirm.IsDismissed || confirm.Value != "true")
        {
            IsRunning = false;
            return;
        }

        //Paso 3: equipo por equipo. Una conexion Mikrotik por servidor.
        ToProcess = Check.ToSuspend;
        var servers = Check.Servers;
        var cortados = 0;
        var saltados = 0;
        var fuera = new List<CorteRunIssueDto>();

        for (var i = 0; i < servers.Count; i++)
        {
            var server = servers[i];
            CurrentServer = Localizer["Corte_ProgressServer", i + 1, servers.Count, server.ServerName].Value;
            StateHasChanged();

            var responseHttp = await _repository.PostAsync<object, CorteRunResultDto>(
                $"{BaseUrl}/{Id}/run/server/{server.ServerId}",
                new { });

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                //Se corta aqui: lo de los equipos anteriores ya quedo hecho
                IsRunning = false;
                await MostrarResumenAsync(cortados, saltados, fuera, false);
                return;
            }

            var parcial = responseHttp.Response ?? new CorteRunResultDto();
            cortados += parcial.Suspended;
            saltados += parcial.Skipped;
            fuera.AddRange(parcial.Issues);

            Processed += server.Contracts;
            StateHasChanged();
        }

        //Paso 4: cerrar el corte
        var finish = await _repository.PostAsync($"{BaseUrl}/{Id}/run/finish", new { });
        IsRunning = false;
        CurrentServer = null;

        if (await _responseHandler.HandleErrorAsync(finish))
        {
            return;
        }

        await MostrarResumenAsync(cortados, saltados, fuera, true);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    //El resumen de lo que paso
    private async Task MostrarResumenAsync(int cortados, int saltados, List<CorteRunIssueDto> fuera, bool completo)
    {
        var texto = $"{Localizer["Corte_ResultSuspended", cortados]}\n{Localizer["Corte_ResultSkipped", saltados]}";

        if (fuera.Count > 0)
        {
            texto += "\n" + Localizer["Corte_ResultIssues", fuera.Count] + "\n" +
                     string.Join("\n", fuera.Take(10).Select(x => $"#{x.ControlContrato} {x.ClientFullName}: {x.Reason}"));
        }

        if (!completo)
        {
            texto += "\n" + Localizer["Corte_Interrupted"];
        }

        await _sweetAlert.FireAsync(
            Localizer[completo ? "Corte_Done" : "Corte_Interrupted"],
            texto,
            fuera.Count > 0 || !completo ? SweetAlertIcon.Warning : SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
