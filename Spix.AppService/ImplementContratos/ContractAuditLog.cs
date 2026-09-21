using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppService.ImplementContratos;

//La unica puerta para anotar en la bitacora del contrato. La usan todos los modulos:
//el registro del contrato, el control de estados, la firma, la suspension y las exoneraciones.
//
//No guarda: quien llama hace SaveChanges dentro de su transaccion, igual que los Registry.
//Asi, si el proceso se cae a mitad, tampoco queda el renglon de la bitacora.
public static class ContractAuditLog
{
    public static async Task AddAsync(
        DataContext context,
        Guid contractClientId,
        ContractEventType eventType,
        string? detail = null,
        string? userName = null,
        Guid? userId = null,
        Guid? referenceId = null,
        string? sourceIp = null,
        string? userAgent = null,
        Guid? clientId = null,
        int? corporationId = null)
    {
        //El contrato manda: de el salen el cliente y la corporacion cuando no vienen dados
        if (clientId == null || corporationId == null)
        {
            var datos = await context.ContractClients
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId)
                .Select(x => new { x.ClientId, x.CorporationId })
                .FirstOrDefaultAsync();

            if (datos == null)
                return;

            clientId ??= datos.ClientId;
            corporationId ??= datos.CorporationId;
        }

        context.ContractAudits.Add(new ContractAudit
        {
            ContractAuditId = Guid.NewGuid(),
            ContractClientId = contractClientId,
            ClientId = clientId.Value,
            DateEvent = DateTime.UtcNow,
            EventType = eventType,
            Detail = Recortar(detail, 256),
            ReferenceId = referenceId,
            SourceIp = Recortar(sourceIp, 64),
            UserAgent = Recortar(userAgent, 512),
            UserId = userId,
            UserByName = Recortar(userName, 150),
            CorporationId = corporationId.Value
        });
    }

    //Un detalle largo no puede tumbar el proceso que se esta auditando
    private static string? Recortar(string? valor, int largo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.Length <= largo ? valor : valor.Substring(0, largo);
    }
}
