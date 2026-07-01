using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface IScheduleServiceX
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatusAsync(string username);
    Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id);
    Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? technicianId);
    Task<ActionResponse<ScheduleItemDto>> CreateAsync(ScheduleItemDto dto, string UserName);
    Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto);
    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
