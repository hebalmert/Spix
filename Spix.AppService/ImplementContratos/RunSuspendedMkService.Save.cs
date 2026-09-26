using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

// Guarda lo que el ESCRITORIO ya escribio en el MikroTik.
//
// Cuando se llama a esto el equipo YA les quito el acceso a los contratos que vienen en la
// lista, escrito por la red LAN. Aqui solo queda el rastro: el contrato pasa a Suspendido,
// se anota el renglon del corte y se abre el registro de la suspension con origen Corte.
//
// Un equipo, una transaccion: si esto falla, lo de los equipos anteriores ya quedo hecho y
// el corte sigue abierto para continuarlo sin repetir a nadie.
public partial class RunSuspendedMkService
{
    public async Task<ActionResponse<CorteRunResultDto>> RunSaveAsync(
        Guid id,
        Guid serverId,
        List<Guid> suspendidos,
        string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<CorteRunResultDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var corporationId = Convert.ToInt32(user.CorporationId);

            var run = await _context.RunSuspendeds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id && x.CorporationId == corporationId);

            if (run == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<CorteRunResultDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (run.Executed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<CorteRunResultDto>(_localizer["Corte_AlreadyExecuted"]);
            }

            //Se vuelve a calcular quien entra en el lote: lo que diga el escritorio se CRUZA
            //con esto, nunca se acepta a ojos cerrados
            var delLote = await ContratosDelLoteAsync(corporationId, serverId);
            var result = new CorteRunResultDto { Contracts = delLote.Count };

            var contracts = delLote
                .Where(x => suspendidos.Contains(x.ContractClientId))
                .ToList();

            result.Skipped = delLote.Count - contracts.Count;

            if (contracts.Count == 0)
            {
                await _transactionManager.CommitTransactionAsync();
                return Ok(result);
            }

            var ids = contracts.Select(x => x.ContractClientId).ToList();

            //La deuda que se esta cobrando: la nota mas vieja con saldo es la que queda
            //anotada en el renglon del corte
            var deudas = await DeudaPorContratoAsync(corporationId, ids);

            //El valor del plan al momento del corte, en una sola consulta
            var planes = (await _context.ContractPlans
                .AsNoTracking()
                .Where(x => ids.Contains(x.ContractClientId))
                .Select(x => new { x.ContractClientId, x.Plan!.Price })
                .ToListAsync())
                .GroupBy(x => x.ContractClientId)
                .ToDictionary(x => x.Key, x => x.First().Price);

            //El tipo del binding tambien queda en la base, igual que lo dejo el equipo
            if (await _contractActivationIntegrityService.UsesHotSpotControlAsync(corporationId))
            {
                var regularType = await _context.HotSpotTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Active && x.TypeName == "regular");

                if (regularType != null)
                {
                    var bindings = await _context.ContractBinds
                        .Where(x => ids.Contains(x.ContractClientId))
                        .ToListAsync();

                    foreach (var binding in bindings)
                    {
                        binding.HotSpotTypeId = regularType.HotSpotTypeId;
                    }
                }
            }

            //Los contratos hay que traerlos RASTREADOS: los de arriba vienen AsNoTracking
            var paraGuardar = await _context.ContractClients
                .Where(x => x.CorporationId == corporationId && ids.Contains(x.ContractClientId))
                .ToListAsync();

            var utcNow = DateTime.UtcNow;
            var userName = $"{user.FirstName} {user.LastName}".Trim();
            var userId = Guid.TryParse(user.Id, out var parsed) ? parsed : (Guid?)null;

            foreach (var contract in paraGuardar)
            {
                deudas.TryGetValue(contract.ContractClientId, out var cxCBillId);
                planes.TryGetValue(contract.ContractClientId, out var planAmount);

                contract.ContractState = ContractState.Suspended;

                _context.RunSuspendedDetails.Add(new RunSuspendedDetail
                {
                    RunSuspendedDetailId = Guid.NewGuid(),
                    RunSuspendedId = run.RunSuspendedId,
                    ContractClientId = contract.ContractClientId,
                    ClientId = contract.ClientId,
                    CxCBillId = cxCBillId,
                    DateUtc = utcNow,
                    PlanAmount = planAmount
                });
            }

            //Queda el registro de la suspension, igual que cuando se hace a mano
            await ContractSuspendedRegistry.OpenManyAsync(_context, paraGuardar, SuspendedOrigin.Corte,
                null, run.RunSuspendedId, userName, userId);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            result.Suspended = paraGuardar.Count;

            return Ok(result);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CorteRunResultDto>(ex);
        }
    }

    // La nota mas vieja con saldo de cada contrato: es la que se anota en el corte
    private async Task<Dictionary<Guid, Guid>> DeudaPorContratoAsync(int corporationId, List<Guid> ids)
    {
        var notas = await _context.CxCBills
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        !x.Cancelled &&
                        x.Balance > 0 &&
                        ids.Contains(x.ContractClientId))
            .Select(x => new { x.CxCBillId, x.ContractClientId, x.DateNote })
            .ToListAsync();

        return notas
            .GroupBy(x => x.ContractClientId)
            .ToDictionary(x => x.Key, x => x.OrderBy(n => n.DateNote).First().CxCBillId);
    }
}
