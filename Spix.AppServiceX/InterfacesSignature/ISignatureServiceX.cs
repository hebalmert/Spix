using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesSignature;

public interface ISignatureServiceX
{
    Task<ActionResponse<IEnumerable<ContractDocumentTemplate>>> GetTemplatesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractDocumentTemplate>> GetTemplateAsync(Guid id, string username);

    Task<ActionResponse<ContractDocumentTemplate>> AddTemplateAsync(ContractDocumentTemplate model, string username);

    Task<ActionResponse<ContractDocumentTemplate>> UpdateTemplateAsync(ContractDocumentTemplate model, string username);

    Task<ActionResponse<ContractDocumentTestDTO>> TestTemplateAsync(Guid templateId, string username);

    Task<ActionResponse<ContractDocumentPdfDTO>> GetTemplatePdfAsync(Guid id, string username);

    Task<ActionResponse<ContractDocumentPdfDTO>> PreviewTemplateAsync(Guid id, List<ContractDocumentTemplateField> fields, string username);

    Task<ActionResponse<IEnumerable<ContractDocumentTemplateField>>> SaveTemplateFieldsAsync(Guid id, List<ContractDocumentTemplateField> fields, string username);

    Task<ActionResponse<bool>> DeleteTemplateAsync(Guid id, string username);

    Task<ActionResponse<ContractDocumentTemplateField>> AddTemplateFieldAsync(ContractDocumentTemplateField model, string username);

    Task<ActionResponse<bool>> DeleteTemplateFieldAsync(Guid id, string username);

    //Portal del cliente (firma Part 11)
    Task<ActionResponse<IEnumerable<MySignatureDocumentDTO>>> GetMyDocumentsAsync(string username);

    Task<ActionResponse<SignatureCodeDTO>> RequestSignatureCodeAsync(Guid contractClientId, ContractDocumentType documentType, ClaimsDTOs context);

    Task<ActionResponse<bool>> SignMyDocumentAsync(SignDocumentRequestDTO model, string urlFront, ClaimsDTOs context);

    Task<ActionResponse<bool>> SendSignatureRequestAsync(Guid contractClientId, string urlFront, ClaimsDTOs context);

    //Enlace del PDF con vigencia corta, pedido al abrir el documento
    Task<ActionResponse<SignatureLinkDTO>> GetMyDocumentLinkAsync(Guid contractClientId, ContractDocumentType documentType, ClaimsDTOs context);

    //Verificacion publica: no pide sesion
    Task<ActionResponse<SignatureVerificationDTO>> VerifySignatureAsync(string verificationCode, ClaimsDTOs context);

    Task<ActionResponse<IEnumerable<ContractSignedDocument>>> GetContractDocumentsAsync(Guid contractClientId, string username);

    Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentAsync(Guid contractClientId, Guid templateId, string username);

    Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentByTypeAsync(Guid contractClientId, ContractDocumentType documentType, string username);

    Task<ActionResponse<ContractSignedDocument>> SignContractDocumentAsync(ContractSignedDocument model, string username);
}
