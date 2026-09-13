using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppService.ImplementContratos;

//Requisitos para aprobar un contrato: foto frontal y trasera del documento, Consentimiento firmado y Contrato firmado.
//Una sola regla para todos: el listado (boton Aprobar), el Edit (bloqueo de In Progress),
//la firma y las fotos (paso automatico de Draft a Pending Approval).
public static class ContractRequirementRules
{
    //Lo que le falta al contrato; lista vacia = completo
    public static async Task<List<string>> GetMissingAsync(DataContext context, Guid contractClientId)
    {
        var pic = await context.ContractIDPics
            .AsNoTracking()
            .Where(x => x.ContractClientId == contractClientId)
            .Select(x => new { x.PhotoIDFront, x.PhotoIDBack })
            .FirstOrDefaultAsync();

        var signedTypes = await context.ContractSignedDocuments
            .AsNoTracking()
            .Where(x => x.ContractClientId == contractClientId && x.Signed)
            .Select(x => x.DocumentType)
            .Distinct()
            .ToListAsync();

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(pic?.PhotoIDFront))
            missing.Add("foto frontal del documento");

        if (string.IsNullOrWhiteSpace(pic?.PhotoIDBack))
            missing.Add("foto trasera del documento");

        if (!signedTypes.Contains(ContractDocumentType.ConsentData))
            missing.Add("Consentimiento firmado");

        if (!signedTypes.Contains(ContractDocumentType.Contract))
            missing.Add("Contrato firmado");

        return missing;
    }

    //Contratos (de la lista dada) que ya tienen todo; se usa para pintar el boton Aprobar sin consultar fila por fila
    public static async Task<HashSet<Guid>> GetCompleteIdsAsync(DataContext context, List<Guid> contractClientIds)
    {
        var withPhotos = await context.ContractIDPics
            .AsNoTracking()
            .Where(x => contractClientIds.Contains(x.ContractClientId) &&
                        x.PhotoIDFront != null && x.PhotoIDFront != "" &&
                        x.PhotoIDBack != null && x.PhotoIDBack != "")
            .Select(x => x.ContractClientId)
            .ToListAsync();

        var signed = await context.ContractSignedDocuments
            .AsNoTracking()
            .Where(x => contractClientIds.Contains(x.ContractClientId) && x.Signed)
            .Select(x => new { x.ContractClientId, x.DocumentType })
            .Distinct()
            .ToListAsync();

        return withPhotos
            .Where(id => signed.Any(x => x.ContractClientId == id && x.DocumentType == ContractDocumentType.ConsentData) &&
                         signed.Any(x => x.ContractClientId == id && x.DocumentType == ContractDocumentType.Contract))
            .ToHashSet();
    }

    //Draft -> Pending Approval cuando ya tiene todo. No guarda: quien llama hace SaveChanges dentro de su transaccion.
    public static async Task PromoteWhenCompleteAsync(DataContext context, Guid contractClientId)
    {
        var contract = await context.ContractClients
            .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

        if (contract == null || contract.ContractState != ContractState.Draft)
            return;

        var missing = await GetMissingAsync(context, contractClientId);
        if (missing.Count == 0)
            contract.ContractState = ContractState.PendingApproval;
    }
}
