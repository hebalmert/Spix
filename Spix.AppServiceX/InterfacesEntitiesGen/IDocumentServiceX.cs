using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesEntitiesGen;

public interface IDocumentServiceX
{
    Task<ActionResponse<IEnumerable<DocumentType>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<DocumentType>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<DocumentType>> GetAsync(Guid id);

    Task<ActionResponse<DocumentType>> UpdateAsync(DocumentType modelo);

    Task<ActionResponse<DocumentType>> AddAsync(DocumentType modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}1