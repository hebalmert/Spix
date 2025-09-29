using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class DocumentTypeUnitOfWork : IDocumentTypeUnitOfWork
{
    private readonly IDocumentTypeService _documentTypeService;

    public DocumentTypeUnitOfWork(IDocumentTypeService documentTypeService)
    {
        _documentTypeService = documentTypeService;
    }

    public async Task<ActionResponse<IEnumerable<DocumentType>>> ComboAsync(string username) => await _documentTypeService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<DocumentType>>> GetAsync(PaginationDTO pagination, string username) => await _documentTypeService.GetAsync(pagination, username);

    public async Task<ActionResponse<DocumentType>> GetAsync(Guid id) => await _documentTypeService.GetAsync(id);

    public async Task<ActionResponse<DocumentType>> UpdateAsync(DocumentType modelo) => await _documentTypeService.UpdateAsync(modelo);

    public async Task<ActionResponse<DocumentType>> AddAsync(DocumentType modelo, string username) => await _documentTypeService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _documentTypeService.DeleteAsync(id);
}