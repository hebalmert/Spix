using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesEntitiesGen;

public interface IRegisterServiceX
{
    Task<ActionResponse<IEnumerable<Register>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Register>> GetAsync(Guid id);

    Task<ActionResponse<Register>> UpdateAsync(Register modelo);

    Task<ActionResponse<Register>> AddAsync(Register modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}