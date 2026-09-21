using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.MyServiceRequestPage;

//El portal del cliente: pide la visita y ve como quedo. El backend ya limita lo que
//puede ver a sus propios contratos.
public partial class IndexMyServiceRequest
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/myservicerequests";

    private List<MyServiceRequestItemDto>? Requests;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var responseHttp = await _repository.GetAsync<List<MyServiceRequestItemDto>>(BaseUrl);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Requests = responseHttp.Response ?? new();
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync()
    {
        await _modalService.ShowAsync(typeof(CreateMyServiceRequest), null, async result =>
        {
            if (result.Succeeded)
            {
                await LoadAsync();
                await _sweetAlert.FireAsync(
                    Localizer["MyRequest_New"],
                    Localizer["MyRequest_Sent"],
                    SweetAlertIcon.Success);
            }
        });
    }

    //Como quedo la visita: comentario, recomendacion y las fotos del despues
    private async Task ShowResultAsync(MyServiceRequestItemDto item)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Item", item }
        };

        await _modalService.ShowAsync(typeof(MyServiceRequestResult), parameters);
    }
}
