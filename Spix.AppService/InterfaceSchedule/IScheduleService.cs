using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceSchedule;

public interface IScheduleService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatusAsync(string username);
    Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id);
    Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? usuarioId);
    Task<ActionResponse<ScheduleItemDto>> CreateAsync(ScheduleItemDto dto, string UserName);
    Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto);
    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
