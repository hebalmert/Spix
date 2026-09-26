using Microsoft.EntityFrameworkCore;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

// Guarda lo que el ESCRITORIO ya escribio en el MikroTik.
//
// Cuando se llama a esto el equipo YA quedo con el binding en regular (al suspender) o en
// bypassed (al reactivar), escrito por la red LAN. Aqui solo queda el rastro en la base:
// el estado del contrato, el registro de la suspension, la bitacora y el tipo del binding.
//
// Las reglas se vuelven a revisar: entre que se pidieron los datos y se guarda pudo pasar
// cualquier cosa, y el escritorio no es quien decide si se podia o no.
public partial class ContractSuspendedMkService
{
    public async Task<ActionResponse<bool>> SuspendSaveAsync(Guid contractClientId, string? motivo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var contract = await _context.ContractClients
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (contract.ContractState != ContractState.Active)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["Suspend_OnlyActive"]);
            }

            //El tipo del binding tambien queda en la base, igual que lo dejo el equipo
            if (await _contractActivationIntegrityService.UsesHotSpotControlAsync(contract.CorporationId))
            {
                var regularType = await _context.HotSpotTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Active && x.TypeName == "regular");

                if (regularType == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<bool>(_localizer["Suspend_NoRegularType"]);
                }

                var bindings = await _context.ContractBinds
                    .Where(x => x.ContractClientId == contractClientId)
                    .ToListAsync();

                foreach (var binding in bindings)
                {
                    binding.HotSpotTypeId = regularType.HotSpotTypeId;
                }
            }

            //El contrato pasa a Suspendido
            contract.ContractState = ContractState.Suspended;

            //Y queda el registro de la suspension
            var userId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
            var userName = $"{user.FirstName} {user.LastName}".Trim();

            await ContractSuspendedRegistry.OpenAsync(_context, contract, SuspendedOrigin.Manual,
                motivo, null, userName, userId);

            await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Suspended,
                motivo, userName, userId, clientId: contract.ClientId, corporationId: contract.CorporationId);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Ok(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<bool>> ReactivateSaveAsync(Guid contractClientId, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var contract = await _context.ContractClients
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (contract.ContractState != ContractState.Suspended)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>("El contrato ya no se encuentra suspendido.");
            }

            //La misma validacion de integridad que corre la web
            var integridad = await _contractActivationIntegrityService.ValidateAsync(
                contract.ContractClientId, contract.CorporationId);

            if (!integridad.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(integridad.Message!);
            }

            if (await _contractActivationIntegrityService.UsesHotSpotControlAsync(contract.CorporationId))
            {
                var bypassedType = await _context.HotSpotTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Active && x.TypeName == "bypassed");

                if (bypassedType == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<bool>(_localizer["Suspend_NoBypassedType"]);
                }

                var bindings = await _context.ContractBinds
                    .Where(x => x.ContractClientId == contractClientId)
                    .ToListAsync();

                foreach (var binding in bindings)
                {
                    binding.HotSpotTypeId = bypassedType.HotSpotTypeId;
                }
            }

            contract.ContractState = ContractState.Active;

            var auditUserId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
            var auditUserName = $"{user.FirstName} {user.LastName}".Trim();

            //Se cierra la suspension abierta: la fila queda como historia, no se borra
            await ContractSuspendedRegistry.CloseAsync(_context, contract.ContractClientId,
                auditUserName, auditUserId);

            await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Reactivated,
                null, auditUserName, auditUserId, clientId: contract.ClientId,
                corporationId: contract.CorporationId);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Ok(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}
