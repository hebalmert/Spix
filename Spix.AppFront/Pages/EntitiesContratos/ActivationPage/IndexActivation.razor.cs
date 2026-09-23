using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ActivationPage;

//La reactivacion de los que se cortaron por falta de pago y ya pagaron.
//Se corre equipo por equipo: una sola conexion Mikrotik por servidor.
public partial class IndexActivation
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private const string BaseUrl = "api/v1/activation";
    private const int PageSize = 15;

    private ActivationCheckDto? Check { get; set; }
    private List<ActivationDetailDto>? Pending { get; set; }
    private int CurrentPage = 1;
    private int TotalPages;

    //El avance de la reactivacion, equipo por equipo
    private bool IsRunning;
    private int ToProcess;
    private int Processed;
    private string? CurrentServer;

    private int Percent => ToProcess == 0 ? 0 : Math.Min(100, Processed * 100 / ToProcess);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await LoadCheckAsync();
        await LoadPendingAsync();
    }

    private async Task LoadCheckAsync()
    {
        var responseHttp = await _repository.GetAsync<ActivationCheckDto>($"{BaseUrl}/check");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Check = responseHttp.Response;
    }

    private async Task LoadPendingAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<ActivationDetailDto>>(
            $"{BaseUrl}/pending?page={page}&recordsnumber={PageSize}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Pending = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadPendingAsync(page);
    }

    //Un solo boton: confirma y reactiva equipo por equipo mostrando el avance.
    //Cada equipo se confirma solo: si uno no responde, lo reactivado queda reactivado.
    private async Task RunAsync()
    {
        if (Check is null || IsRunning)
            return;

        IsRunning = true;
        Processed = 0;
        ToProcess = 0;
        CurrentServer = null;

        //Se vuelve a revisar, por si alguien mas reactivo o pago mientras tanto
        await LoadCheckAsync();
        StateHasChanged();

        if (Check is null || Check.ToActivate == 0)
        {
            IsRunning = false;
            await _sweetAlert.FireAsync(Localizer["Activation_Run"], Localizer["Activation_Nothing"], SweetAlertIcon.Info);
            return;
        }

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Activation_Run"],
            Text = Localizer["Activation_ConfirmText", Check.ToActivate].Value,
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Activation_Run"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirm.IsDismissed || confirm.Value != "true")
        {
            IsRunning = false;
            return;
        }

        //Equipo por equipo. Una conexion Mikrotik por servidor.
        ToProcess = Check.ToActivate;
        var servers = Check.Servers;
        var activados = 0;
        var saltados = 0;
        var fuera = new List<ActivationIssueDto>();

        for (var i = 0; i < servers.Count; i++)
        {
            var server = servers[i];
            CurrentServer = Localizer["Corte_ProgressServer", i + 1, servers.Count, server.ServerName].Value;
            StateHasChanged();

            var responseHttp = await _repository.PostAsync<object, ActivationRunResultDto>(
                $"{BaseUrl}/run/server/{server.ServerId}",
                new { });

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                //Se corta aqui: lo de los equipos anteriores ya quedo hecho
                IsRunning = false;
                await ShowResultAsync(activados, saltados, fuera, false);
                await ReloadAsync();
                return;
            }

            var parcial = responseHttp.Response ?? new ActivationRunResultDto();
            activados += parcial.Activated;
            saltados += parcial.Skipped;
            fuera.AddRange(parcial.Issues);

            Processed += server.Contracts;
            StateHasChanged();
        }

        IsRunning = false;
        CurrentServer = null;

        await ShowResultAsync(activados, saltados, fuera, true);
        await ReloadAsync();
    }

    private async Task ShowResultAsync(int activados, int saltados, List<ActivationIssueDto> fuera, bool completo)
    {
        var texto = $"{Localizer["Activation_ResultOk", activados]}\n{Localizer["Corte_ResultSkipped", saltados]}";

        if (fuera.Count > 0)
        {
            texto += "\n" + Localizer["Corte_ResultIssues", fuera.Count];
        }

        if (!completo)
        {
            texto += "\n" + Localizer["Activation_Interrupted"];
        }

        await _sweetAlert.FireAsync(
            Localizer[completo ? "Activation_Done" : "Activation_Interrupted"],
            texto,
            fuera.Count > 0 || !completo ? SweetAlertIcon.Warning : SweetAlertIcon.Success);
    }

    private async Task ReloadAsync()
    {
        await LoadCheckAsync();
        await LoadPendingAsync();
    }
}
