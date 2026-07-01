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

    public Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id) => _scheduleService.GetByIdAsync(id);

    public Task<ActionResponse<ScheduleItemDto>> CreateAsync(ScheduleItemDto dto, string UserName) => _scheduleService.CreateAsync(dto, UserName);

    public Task<ActionResponse<bool>> DeleteAsync(Guid id) => _scheduleService.DeleteAsync(id);

    public Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? technicianId) => _scheduleService.GetAsync(fromUtc, toUtc, technicianId);

    public Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto) => _scheduleService.UpdateAsync(id, dto);

}
