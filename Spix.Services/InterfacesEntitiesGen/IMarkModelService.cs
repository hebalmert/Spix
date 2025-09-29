using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesGen;

public interface IMarkModelService
{
    Task<ActionResponse<IEnumerable<MarkModel>>> ComboAsync(string emausernameil, Guid id);

    Task<ActionResponse<IEnumerable<MarkModel>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<MarkModel>> GetAsync(Guid id);

    Task<ActionResponse<MarkModel>> UpdateAsync(MarkModel modelo);

    Task<ActionResponse<MarkModel>> AddAsync(MarkModel modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}