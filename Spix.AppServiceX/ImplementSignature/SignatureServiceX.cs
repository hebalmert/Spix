using Spix.AppService.InterfacesSignature;
using Spix.AppServiceX.InterfacesSignature;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementSignature;

public class SignatureServiceX : ISignatureServiceX
{
    private readonly ISignatureService _signatureService;

    public SignatureServiceX(ISignatureService signatureService)
    {
        _signatureService = signatureService;
    }

    public async Task<ActionResponse<IEnumerable<ContractDocumentTemplate>>> GetTemplatesAsync(PaginationDTO pagination, string username) =>
        await _signatureService.GetTemplatesAsync(pagination, username);

    public async Task<ActionResponse<ContractDocumentTemplate>> GetTemplateAsync(Guid id, string username) =>
        await _signatureService.GetTemplateAsync(id, username);

    public async Task<ActionResponse<ContractDocumentTemplate>> AddTemplateAsync(ContractDocumentTemplate model, string username) =>
        await _signatureService.AddTemplateAsync(model, username);

    public async Task<ActionResponse<ContractDocumentTemplate>> UpdateTemplateAsync(ContractDocumentTemplate model, string username) =>
        await _signatureService.UpdateTemplateAsync(model, username);

    public async Task<ActionResponse<ContractDocumentTestDTO>> TestTemplateAsync(Guid templateId, string username) =>
        await _signatureService.TestTemplateAsync(templateId, username);

    public async Task<ActionResponse<bool>> DeleteTemplateAsync(Guid id, string username) =>
        await _signatureService.DeleteTemplateAsync(id, username);

    public async Task<ActionResponse<ContractDocumentTemplateField>> AddTemplateFieldAsync(ContractDocumentTemplateField model, string username) =>
        await _signatureService.AddTemplateFieldAsync(model, username);

    public async Task<ActionResponse<bool>> DeleteTemplateFieldAsync(Guid id, string username) =>
        await _signatureService.DeleteTemplateFieldAsync(id, username);

    public async Task<ActionResponse<IEnumerable<ContractSignedDocument>>> GetContractDocumentsAsync(Guid contractClientId, string username) =>
        await _signatureService.GetContractDocumentsAsync(contractClientId, username);

    public async Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentAsync(Guid contractClientId, Guid templateId, string username) =>
        await _signatureService.GenerateContractDocumentAsync(contractClientId, templateId, username);

    public async Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentByTypeAsync(Guid contractClientId, ContractDocumentType documentType, string username) =>
        await _signatureService.GenerateContractDocumentByTypeAsync(contractClientId, documentType, username);

    public async Task<ActionResponse<ContractSignedDocument>> SignContractDocumentAsync(ContractSignedDocument model, string username) =>
        await _signatureService.SignContractDocumentAsync(model, username);
}
