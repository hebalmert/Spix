using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesSignature;

public interface ISignatureService
{
    Task<ActionResponse<IEnumerable<ContractDocumentTemplate>>> GetTemplatesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractDocumentTemplate>> GetTemplateAsync(Guid id, string username);

    Task<ActionResponse<ContractDocumentTemplate>> AddTemplateAsync(ContractDocumentTemplate model, string username);

    Task<ActionResponse<ContractDocumentTemplate>> UpdateTemplateAsync(ContractDocumentTemplate model, string username);

    Task<ActionResponse<ContractDocumentTestDTO>> TestTemplateAsync(Guid templateId, string username);

    Task<ActionResponse<bool>> DeleteTemplateAsync(Guid id, string username);

    Task<ActionResponse<ContractDocumentTemplateField>> AddTemplateFieldAsync(ContractDocumentTemplateField model, string username);

    Task<ActionResponse<bool>> DeleteTemplateFieldAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<ContractSignedDocument>>> GetContractDocumentsAsync(Guid contractClientId, string username);

    Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentAsync(Guid contractClientId, Guid templateId, string username);

    Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentByTypeAsync(Guid contractClientId, ContractDocumentType documentType, string username);

    Task<ActionResponse<ContractSignedDocument>> SignContractDocumentAsync(ContractSignedDocument model, string username);
}
