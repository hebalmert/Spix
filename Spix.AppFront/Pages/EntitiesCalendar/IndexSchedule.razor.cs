using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesCalendar;

public partial class IndexSchedule
{
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    //La lista del filtro la arma el backend, con "Todos" en la posicion 0
    private const string BaseComboStatus = "/api/v1/schedulecontrol/loadStatusFilter";

    private SpixCalendar? _calendar;
    private List<IntItemModel>? Statuses;
    private int StatusFilter;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>(BaseComboStatus);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Statuses = responseHttp.Response ?? new();
    }

    //Cero es "todos": el calendario vuelve a mostrar la agenda completa
    private async Task StatusChanged(ChangeEventArgs e)
    {
        StatusFilter = int.TryParse(e.Value?.ToString(), out var valor) ? valor : 0;

        if (_calendar is not null)
        {
            await _calendar.SetStatusFilterAsync(StatusFilter);
        }
    }

    private async Task ShowModalAsync(Guid? id, bool isEdit)
    {
        Type component;
        Dictionary<string, object> parameters;

        if (isEdit)
        {
            component = typeof(EditSchedule);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Schedule)]}" }
            };
        }
        else
        {
            component = typeof(CreateSchedule);
            parameters = new Dictionary<string, object>
            {
                { "Title", $"{Localizer[nameof(Resource.Create_Schedule)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded && _calendar is not null)
            {
                await _calendar.ReloadAsync();
            }
        });
    }
}
