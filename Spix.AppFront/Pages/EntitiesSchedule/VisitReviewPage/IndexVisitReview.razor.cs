using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.VisitReviewPage;

//La bandeja de revision. Dos pestañas, dos endpoints: no se filtra en el front.
public partial class IndexVisitReview
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "/api/v1/visitreviews";

    //0 = Ubicacion, 1 = Sin cliente
    private int Tab;

    private string Filter { get; set; } = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 10;
    private int TotalRecords;

    private VisitReviewCountersDto? Counters;
    private List<VisitReviewDto>? Items;

    protected override async Task OnInitializedAsync()
    {
        await LoadCountersAsync();
        await Cargar();
    }

    //El contador se recarga solo al abrir y despues de cada cambio, no en cada pintada
    private async Task LoadCountersAsync()
    {
        var responseHttp = await _repository.GetAsync<VisitReviewCountersDto>($"{BaseUrl}/counters");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Counters = responseHttp.Response;
    }

    private async Task SetTabAsync(int tab)
    {
        Tab = tab;
        await Cargar();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await Cargar();
    }

    private async Task Cargar(int page = 1)
    {
        Items = null;
        CurrentPage = page;

        var ruta = Tab == 0 ? "location" : "absent";
        var url = $"{BaseUrl}/{ruta}?page={page}&recordsnumber={PageSize}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Uri.EscapeDataString(Filter)}";
        }

        var responseHttp = await _repository.GetAsync<List<VisitReviewDto>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Items = responseHttp.Response ?? new();
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault() ?? "1");

        if (responseHttp.HttpResponseMessage.Headers.TryGetValues("Counting", out var counting) &&
            int.TryParse(counting.FirstOrDefault(), out var total))
        {
            TotalRecords = total;
        }
    }

    //Aplicar escribe la coordenada del tecnico en el contrato, sin transcribir nada a mano
    private async Task ApplyAsync(VisitReviewDto item)
    {
        var confirma = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Aplicar ubicacion",
            Text = $"La ubicacion del contrato {item.ControlContrato} queda en la que tomo el tecnico.",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.ButtonSave)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirma.IsDismissed || confirma.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync<object, bool>($"{BaseUrl}/{item.ServiceRequestId}/applylocation", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Revision", "Ubicacion aplicada al contrato.", SweetAlertIcon.Success);
        await RecargarAsync();
    }

    //Descartar deja el contrato como esta y saca la visita de la bandeja
    private async Task DismissAsync(VisitReviewDto item)
    {
        var confirma = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Dejar como esta",
            Text = "La ubicacion del contrato no cambia y la visita sale de la bandeja.",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.ButtonSave)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirma.IsDismissed || confirma.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync<object, bool>($"{BaseUrl}/{item.ServiceRequestId}/dismiss", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await RecargarAsync();
    }

    private async Task RescheduleAsync(VisitReviewDto item)
    {
        await _modalService.ShowAsync(typeof(RescheduleVisit), new Dictionary<string, object>
        {
            { nameof(RescheduleVisit.ServiceRequestId), item.ServiceRequestId },
            { nameof(RescheduleVisit.RequestNumber), item.RequestNumber },
            { nameof(RescheduleVisit.ClientFullName), item.ClientFullName }
        }, OnRescheduledAsync);
    }

    //El SweetAlert de exito vive aqui, sobre la pantalla ya limpia
    private async Task OnRescheduledAsync(ModalResult result)
    {
        if (!result.Succeeded)
            return;

        await _sweetAlert.FireAsync("Revision", "Visita reagendada.", SweetAlertIcon.Success);
        await RecargarAsync();
    }

    private async Task RecargarAsync()
    {
        await LoadCountersAsync();
        await Cargar(CurrentPage);
    }

    private void GoToOrder(Guid id) => _navigationManager.NavigateTo($"/servicerequests/{id}");

    private static string Coord(decimal? latitude, decimal? longitude)
    {
        if (latitude is null || longitude is null)
            return "sin ubicacion";

        return $"{latitude}, {longitude}";
    }

    private static string DistText(VisitReviewDto item)
    {
        if (item.DistanceMeters is null)
            return "sin comparar";

        return item.SameSite
            ? $"en el sitio - {item.DistanceMeters} m"
            : $"lejos - {item.DistanceMeters} m";
    }

    private static string DistClass(VisitReviewDto item)
    {
        if (item.DistanceMeters is null)
            return "is-none";

        return item.SameSite ? "is-ok" : "is-far";
    }
}
