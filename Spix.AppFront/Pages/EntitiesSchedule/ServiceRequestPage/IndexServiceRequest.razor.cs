using Spix.DomainLogic.ItemsGeneric;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class IndexServiceRequest
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter] public Guid? Id { get; set; }

    private string Filter { get; set; } = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 10;
    private int TotalRecords;

    //El tablero y las pildoras: los numeros y la lista los arma el backend
    private const string BaseComboStatus = "/api/v1/schedulecontrol/loadStatusFilter";

    private ServiceRequestSummaryDto? Summary;
    private List<IntItemModel>? Statuses;
    private int StatusFilter;

    private const string baseUrl = "api/v1/servicerequests";
    public List<ServiceRequestDto>? Requests { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //Quien llegue con /servicerequests/{id} va derecho a la orden
            if (Id.HasValue)
            {
                GoToOrder(Id.Value);
                return;
            }

            await LoadStatusesAsync();
            await LoadSummaryAsync();
            await Cargar();
        }
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
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}&status={StatusFilter}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Uri.EscapeDataString(Filter)}";
        }

        var responseHttp = await _repository.GetAsync<List<ServiceRequestDto>>(url);
        var errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Requests = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault() ?? "1");

        //Cuantas hay en total: el backend ya lo manda en la cabecera
        if (responseHttp.HttpResponseMessage.Headers.TryGetValues("Counting", out var counting) &&
            int.TryParse(counting.FirstOrDefault(), out var total))
        {
            TotalRecords = total;
        }

        await InvokeAsync(StateHasChanged);
    }

    //Nueva solicitud: lo unico que sigue siendo modal, porque es corto
    private async Task ShowModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", "Nueva Solicitud" }
        };

        await _modalService.ShowAsync(typeof(CreateServiceRequest), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar(CurrentPage);
                await LoadSummaryAsync();
            }
        });
    }

    //La orden de trabajo es una pantalla propia
    private void GoToOrder(Guid id)
    {
        _navigationManager.NavigateTo($"/servicerequests/details/{id}");
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await Cargar(CurrentPage);
        await LoadSummaryAsync();
    }

    private async Task LoadStatusesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>(BaseComboStatus);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Statuses = responseHttp.Response ?? new();
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceRequestSummaryDto>($"{baseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Summary = responseHttp.Response;
    }

    //Cero es "todas"; al cambiar de pildora se vuelve a la primera pagina
    private async Task SetStatusAsync(int status)
    {
        StatusFilter = status;
        CurrentPage = 1;
        await Cargar();
    }

    //Rastro de la solicitud: cuando se creo, quien la cerro y cuando.
    private async Task ShowAuditAsync(ServiceRequestDto item)
    {
        await AuditAlert.ShowAsync(_sweetAlert, Localizer["Audit_Title"],
            (Localizer["Audit_Created"], item.CreatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            (Localizer["Audit_Scheduled"], item.ScheduledAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            (Localizer["Audit_Technician"], item.TechnicianName),
            (Localizer["Audit_ClosedBy"], item.UsuarioOwnerCompleted),
            (Localizer["Audit_ClosedDate"], item.CompletedAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")));
    }

    //La fecha programada dice que tan urgente es, no solo cuando es
    private string WhenText(ServiceRequestDto item)
    {
        //Mientras el cliente la pide y nadie la revisa, no hay cuando
        if (item.ScheduleStatus == ScheduleStatus.Requested)
            return Localizer["When_Unassigned"];

        if (item.ScheduleStatus == ScheduleStatus.Completed ||
            item.ScheduleStatus == ScheduleStatus.PhoneResolved)
            return Localizer["When_Closed"];

        if (item.ScheduledAtUtc == null)
            return Localizer["When_Unassigned"];

        var fecha = item.ScheduledAtUtc.Value.ToLocalTime();
        var dias = (fecha.Date - DateTime.Now.Date).Days;

        if (dias == 0)
            return $"{Localizer["When_Today"]} {fecha:HH:mm}";

        if (dias == 1)
            return Localizer["When_Tomorrow"];

        if (dias < 0)
            return $"{Localizer["When_Overdue"]} {Math.Abs(dias)}d";

        return $"{Localizer["When_InDays"]} {dias}d";
    }

    private string WhenClass(ServiceRequestDto item)
    {
        if (item.ScheduleStatus == ScheduleStatus.Completed ||
            item.ScheduleStatus == ScheduleStatus.PhoneResolved)
            return "is-done";

        if (item.ScheduledAtUtc == null)
            return "is-next";

        var dias = (item.ScheduledAtUtc.Value.ToLocalTime().Date - DateTime.Now.Date).Days;

        if (dias < 0)
            return "is-late";

        return dias == 0 ? "is-today" : "is-next";
    }
}
