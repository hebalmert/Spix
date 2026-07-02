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
    private bool OpenedRouteModal;

    private const string baseUrl = "api/v1/servicerequests";
    public List<ServiceRequestDto>? Requests { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();

            if (Id.HasValue)
            {
                OpenedRouteModal = true;
                await ShowModalAsync(Id.Value, true);
            }
        }
        else if (Id.HasValue && !OpenedRouteModal)
        {
            OpenedRouteModal = true;
            await ShowModalAsync(Id.Value, true);
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
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
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

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;

        if (isEdit)
        {
            component = typeof(EditServiceRequest);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", "Editar Solicitud" }
            };
        }
        else
        {
            component = typeof(CreateServiceRequest);
            parameters = new Dictionary<string, object>
            {
                { "Title", "Nueva Solicitud" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar(CurrentPage);
            }

            if (Id.HasValue)
            {
                _navigationManager.NavigateTo("/servicerequests");
            }
        });
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
    }
}
