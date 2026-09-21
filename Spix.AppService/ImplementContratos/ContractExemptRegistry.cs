using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppService.ImplementContratos;

//Abre y cierra el registro de exoneracion. Una sola pieza para los dos caminos:
//el modulo de exonerados y el cambio de estado desde Control de Contratos.
//
//La fila NO se borra al retirar el beneficio: se cierra con DateEnded y queda como historia,
//asi el modulo puede reportar cuantos hay, cuanto suman y quienes lo estuvieron entre dos fechas.
//
//Ninguno de los dos metodos guarda: quien llama hace SaveChanges dentro de su transaccion.
public static class ContractExemptRegistry
{
    //Abre la exoneracion del contrato. Si ya hay una abierta no crea otra.
    public static async Task OpenAsync(DataContext context, ContractClient contract,
        string? motivo, string? userName, Guid? userId)
    {
        var yaAbierta = await context.ContractExempts
            .AnyAsync(x => x.ContractClientId == contract.ContractClientId && x.DateEnded == null);

        if (yaAbierta)
            return;

        //Foto del plan al momento de exonerar: el contrato puede cambiar de plan despues
        var plan = await context.ContractPlans
            .AsNoTracking()
            .Include(x => x.Plan)
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .Select(x => new { x.Plan!.PlanName, x.Plan.Price })
            .FirstOrDefaultAsync();

        //Y el resto de los datos del contrato y del cliente, tal como estan hoy
        var datos = await context.ContractClients
            .AsNoTracking()
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .Select(x => new
            {
                x.ControlContrato,
                x.Address,
                x.PhoneNumber,
                ClientName = x.Client!.FirstName + " " + x.Client.LastName,
                x.Client.Document,
                CityName = x.Zone!.City!.Name,
                x.Zone.ZoneName
            })
            .FirstOrDefaultAsync();

        context.ContractExempts.Add(new ContractExempt
        {
            ContractExemptId = Guid.NewGuid(),
            ContractClientId = contract.ContractClientId,
            ClientId = contract.ClientId,
            DateExempt = DateTime.UtcNow,
            Motivo = motivo,
            ControlContrato = datos?.ControlContrato ?? contract.ControlContrato,
            ClientName = datos?.ClientName,
            ClientDocument = datos?.Document,
            ContractAddress = datos?.Address,
            ContractPhone = datos?.PhoneNumber,
            CityName = datos?.CityName,
            ZoneName = datos?.ZoneName,
            PlanName = plan?.PlanName,
            PlanAmount = plan?.Price ?? 0,
            UserByName = userName,
            UserId = userId,
            CorporationId = contract.CorporationId
        });
    }

    //Cierra la exoneracion abierta del contrato, si la hay.
    public static async Task CloseAsync(DataContext context, Guid contractClientId, string? userName, Guid? userId)
    {
        var abierta = await context.ContractExempts
            .Where(x => x.ContractClientId == contractClientId && x.DateEnded == null)
            .OrderByDescending(x => x.DateExempt)
            .FirstOrDefaultAsync();

        if (abierta == null)
            return;

        abierta.DateEnded = DateTime.UtcNow;
        abierta.UserByNameEnded = userName;
        abierta.UserIdEnded = userId;
    }
}
