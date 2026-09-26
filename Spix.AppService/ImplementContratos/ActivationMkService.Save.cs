using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

// Guarda lo que el ESCRITORIO ya escribio en el MikroTik.
//
// Cuando se llama a esto el equipo YA les devolvio el acceso a los contratos que vienen en
// la lista, escrito por la red LAN. Aqui solo queda el rastro en la base: el contrato pasa
// a Activo, se cierra su suspension y el binding queda con el tipo que tiene el equipo.
//
// Un equipo, una transaccion: es la misma regla de la web. Si esto falla, lo de los equipos
// anteriores ya quedo hecho y el lote se puede volver a lanzar.
//
// NO se escribe evento de bitacora, porque la reactivacion masiva de la web tampoco lo
// escribe: si lo escribiera, lo reactivado desde el escritorio saldria en la pantalla de
// Auditoria de activaciones y lo reactivado desde la web no, y las dos no cuadrarian.
public partial class ActivationMkService
{
    public async Task<ActionResponse<ActivationRunResultDto>> ActivateSaveAsync(
        Guid serverId,
        List<Guid> activados,
        string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ActivationRunResultDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var corporationId = Convert.ToInt32(user.CorporationId);

            //Se vuelve a calcular quien entra en el lote: lo que diga el escritorio se
            //CRUZA con esto, nunca se acepta a ojos cerrados
            var ids = await PendingQuery(corporationId)
                .Where(x => _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId &&
                                                            b.ServerId == serverId) &&
                            _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId))
                .Select(x => x.ContractClientId)
                .ToListAsync();

            var result = new ActivationRunResultDto { Contracts = ids.Count };

            var aceptados = ids.Where(x => activados.Contains(x)).ToList();

            if (aceptados.Count == 0)
            {
                await _transactionManager.CommitTransactionAsync();
                return Ok(result);
            }

            var contracts = await _context.ContractClients
                .Where(x => x.CorporationId == corporationId &&
                            x.ContractState == ContractState.Suspended &&
                            aceptados.Contains(x.ContractClientId))
                .ToListAsync();

            //Los que ya no estaban suspendidos cuando se fue a guardar
            result.Skipped = ids.Count - contracts.Count;

            if (contracts.Count == 0)
            {
                await _transactionManager.CommitTransactionAsync();
                return Ok(result);
            }

            var contractIds = contracts.Select(x => x.ContractClientId).ToList();

            //El tipo del binding tambien queda en la base, igual que lo dejo el equipo
            if (await _contractActivationIntegrityService.UsesHotSpotControlAsync(corporationId))
            {
                var bypassed = await _context.HotSpotTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Active && x.TypeName == "bypassed");

                if (bypassed == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<ActivationRunResultDto>(_localizer["Activation_NoBypassed"]);
                }

                var bindings = await _context.ContractBinds
                    .Where(x => x.ServerId == serverId && contractIds.Contains(x.ContractClientId))
                    .ToListAsync();

                foreach (var binding in bindings)
                {
                    binding.HotSpotTypeId = bypassed.HotSpotTypeId;
                }
            }

            var userName = $"{user.FirstName} {user.LastName}".Trim();
            var userId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;

            foreach (var contract in contracts)
            {
                contract.ContractState = ContractState.Active;

                //Se cierra la suspension: la fila queda como historia, no se borra
                await ContractSuspendedRegistry.CloseAsync(_context, contract.ContractClientId, userName, userId);
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            result.Activated = contracts.Count;

            return Ok(result);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ActivationRunResultDto>(ex);
        }
    }
}
