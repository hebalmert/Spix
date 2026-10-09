using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceSchedule;

public interface ITechVisitService
{
    Task<ActionResponse<IEnumerable<TechVisitDto>>> GetMineAsync(string username);

    Task<ActionResponse<TechVisitDto>> GetAsync(Guid id, string username);

    Task<ActionResponse<TechVisitDto>> StartAsync(Guid id, string username);
}
