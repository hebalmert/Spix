using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface IDocumentTypeUnitOfWork
{
    Task<ActionResponse<IEnumerable<DocumentType>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<DocumentType>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<DocumentType>> GetAsync(Guid id);

    Task<ActionResponse<DocumentType>> UpdateAsync(DocumentType modelo);

    Task<ActionResponse<DocumentType>> AddAsync(DocumentType modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}