using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Globalization;

namespace Spix.AppFront.Pages.EntitiesCalendar;

public partial class CreateSchedule
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }
    [Parameter] public string? SelectedDate { get; set; }


    private ScheduleItemDto Schedule = new() { ScheduleStatus = ScheduleStatus.Pending };
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override void OnInitialized()
    {
        if (SelectedDate != null)
        {
            var date = DateTime.Parse(SelectedDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            Schedule.StartUtc = DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
            Schedule.EndUtc = DateTime.SpecifyKind(date.AddHours(1), DateTimeKind.Local).ToUniversalTime();
        }
    }

    private async Task Create()
    {
        IsSaving = true;
        var response = await _repository.PostAsync("/api/v1/schedulecontrol", Schedule);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(response))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}