using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface IScheduleServiceX
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatusAsync(string username);
    Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id, string username);
    Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? technicianId, string username);
    Task<ActionResponse<ScheduleItemDto>> CreateAsync(ScheduleItemDto dto, string UserName);
    Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto, string username);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
