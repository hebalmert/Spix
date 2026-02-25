using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesEntitiesGen;

public interface IMarkModelServiceX
{
    Task<ActionResponse<IEnumerable<MarkModel>>> ComboAsync(string username, Guid id);

    Task<ActionResponse<IEnumerable<MarkModel>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<MarkModel>> GetAsync(Guid id);

    Task<ActionResponse<MarkModel>> UpdateAsync(MarkModel modelo);

    Task<ActionResponse<MarkModel>> AddAsync(MarkModel modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}