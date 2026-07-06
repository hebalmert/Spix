using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSchedule;

public class ScheduleServiceX : IScheduleServiceX
{
    private readonly IScheduleService _scheduleService;

    public ScheduleServiceX(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatusAsync(string username) => await _scheduleService.ComboStatusAsync(username);

    public Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id, string username) => _scheduleService.GetByIdAsync(id, username);

    public Task<ActionResponse<ScheduleItemDto>> CreateAsync(ScheduleItemDto dto, string UserName) => _scheduleService.CreateAsync(dto, UserName);

    public Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => _scheduleService.DeleteAsync(id, username);

    public Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? technicianId, string username) => _scheduleService.GetAsync(fromUtc, toUtc, technicianId, username);

    public Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto, string username) => _scheduleService.UpdateAsync(id, dto, username);

}
