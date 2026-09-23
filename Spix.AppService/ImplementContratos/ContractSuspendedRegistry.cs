using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppService.ImplementContratos;

//Abre y cierra el registro de suspension. Una sola pieza para los tres caminos:
//el cambio de estado a mano, el corte masivo y la reactivacion desde el modulo de suspendidos.
//
//La fila NO se borra al reactivar: se cierra con DateReactivated y queda como historia,
//asi el modulo puede reportar cuantos hay, cuanto suman y quienes lo estuvieron entre dos fechas.
//
//Ninguno de los dos metodos guarda: quien llama hace SaveChanges dentro de su transaccion.
public static class ContractSuspendedRegistry
{
    //Abre la suspension del contrato. Si ya hay una abierta no crea otra.
    public static async Task OpenAsync(DataContext context, ContractClient contract, SuspendedOrigin origin,
        string? motivo, Guid? runSuspendedId, string? userName, Guid? userId)
    {
        var yaAbierta = await context.ContractSuspendeds
            .AnyAsync(x => x.ContractClientId == contract.ContractClientId && x.DateReactivated == null);

        if (yaAbierta)
            return;

        //Foto del plan al momento de suspender: el contrato puede cambiar de plan despues
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

        //Con que editar el registro en el equipo cuando haya que devolver el acceso
        var bind = await context.ContractBinds
            .AsNoTracking()
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .Select(x => new { x.MikrotikId, x.ServerId })
            .FirstOrDefaultAsync();

        context.ContractSuspendeds.Add(new ContractSuspended
        {
            MkIndex = bind?.MikrotikId,
            ServerId = bind?.ServerId,
            ContractSuspendedId = Guid.NewGuid(),
            ContractClientId = contract.ContractClientId,
            ClientId = contract.ClientId,
            DateSuspended = DateTime.UtcNow,
            Origin = origin,
            RunSuspendedId = runSuspendedId,
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

    //La version por lote del corte: los mismos datos, pero en cuatro consultas para TODOS
    //los contratos del lote y no cuatro por cada uno.
    public static async Task OpenManyAsync(DataContext context, List<ContractClient> contracts, SuspendedOrigin origin,
        string? motivo, Guid? runSuspendedId, string? userName, Guid? userId)
    {
        if (contracts.Count == 0)
            return;

        var ids = contracts.Select(x => x.ContractClientId).ToList();

        //Los que ya tienen una suspension abierta no se vuelven a abrir
        var yaAbiertas = (await context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => ids.Contains(x.ContractClientId) && x.DateReactivated == null)
            .Select(x => x.ContractClientId)
            .ToListAsync()).ToHashSet();

        //Foto del plan al momento de suspender
        var planes = (await context.ContractPlans
            .AsNoTracking()
            .Where(x => ids.Contains(x.ContractClientId))
            .Select(x => new { x.ContractClientId, x.Plan!.PlanName, x.Plan.Price })
            .ToListAsync())
            .GroupBy(x => x.ContractClientId)
            .ToDictionary(x => x.Key, x => x.First());

        //Los datos del contrato y del cliente, tal como estan hoy
        var datos = (await context.ContractClients
            .AsNoTracking()
            .Where(x => ids.Contains(x.ContractClientId))
            .Select(x => new
            {
                x.ContractClientId,
                x.ControlContrato,
                x.Address,
                x.PhoneNumber,
                ClientName = x.Client!.FirstName + " " + x.Client.LastName,
                x.Client.Document,
                CityName = x.Zone!.City!.Name,
                x.Zone.ZoneName
            })
            .ToListAsync())
            .ToDictionary(x => x.ContractClientId);

        //Con que editar el registro en el equipo cuando haya que devolver el acceso
        var binds = (await context.ContractBinds
            .AsNoTracking()
            .Where(x => ids.Contains(x.ContractClientId))
            .Select(x => new { x.ContractClientId, x.MikrotikId, x.ServerId })
            .ToListAsync())
            .GroupBy(x => x.ContractClientId)
            .ToDictionary(x => x.Key, x => x.First());

        var utcNow = DateTime.UtcNow;

        foreach (var contract in contracts)
        {
            if (yaAbiertas.Contains(contract.ContractClientId))
                continue;

            planes.TryGetValue(contract.ContractClientId, out var plan);
            datos.TryGetValue(contract.ContractClientId, out var dato);
            binds.TryGetValue(contract.ContractClientId, out var bind);

            context.ContractSuspendeds.Add(new ContractSuspended
            {
                MkIndex = bind?.MikrotikId,
                ServerId = bind?.ServerId,
                ContractSuspendedId = Guid.NewGuid(),
                ContractClientId = contract.ContractClientId,
                ClientId = contract.ClientId,
                DateSuspended = utcNow,
                Origin = origin,
                RunSuspendedId = runSuspendedId,
                Motivo = motivo,
                ControlContrato = dato?.ControlContrato ?? contract.ControlContrato,
                ClientName = dato?.ClientName,
                ClientDocument = dato?.Document,
                ContractAddress = dato?.Address,
                ContractPhone = dato?.PhoneNumber,
                CityName = dato?.CityName,
                ZoneName = dato?.ZoneName,
                PlanName = plan?.PlanName,
                PlanAmount = plan?.Price ?? 0,
                UserByName = userName,
                UserId = userId,
                CorporationId = contract.CorporationId
            });
        }
    }

    //Cierra la suspension abierta del contrato, si la hay.
    public static async Task CloseAsync(DataContext context, Guid contractClientId, string? userName, Guid? userId)
    {
        var abierta = await context.ContractSuspendeds
            .Where(x => x.ContractClientId == contractClientId && x.DateReactivated == null)
            .OrderByDescending(x => x.DateSuspended)
            .FirstOrDefaultAsync();

        if (abierta == null)
            return;

        abierta.DateReactivated = DateTime.UtcNow;
        abierta.UserByNameReactivated = userName;
        abierta.UserIdReactivated = userId;
    }
}
