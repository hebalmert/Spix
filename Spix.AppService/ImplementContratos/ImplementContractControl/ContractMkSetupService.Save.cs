using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos.ImplementContractControl;

// Guarda lo que el ESCRITORIO ya escribio en el MikroTik.
//
// Cuando se llama a esto el equipo YA quedo configurado, por la red LAN: aqui solo se deja
// el registro en la base. Por eso ninguno de estos metodos abre una conexion al equipo, a
// diferencia de ContractQueService y ContractBindService, que hacen las dos cosas porque
// la web no puede llegar al equipo por su cuenta.
//
// Va en un archivo aparte del que lee los datos para que se vea de un golpe que son dos
// responsabilidades distintas: alli se junta informacion, aqui se persiste.
public partial class ContractMkSetupService
{
    public async Task<ActionResponse<ContractQue>> SaveQueAsync(ContractQueSaveDTO datos, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractQue>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var yaExiste = await _context.ContractQues
                .AnyAsync(x => x.ContractClientId == datos.ContractClientId);

            if (yaExiste)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractQue>("Ya existe una Queue de Velocidad para este contrato.");
            }

            //El queue padre: se crea si el escritorio tuvo que crearlo en el equipo, y si
            //ya estaba solo se le actualizan las velocidades, que cambian al colgarle un
            //cliente mas.
            var padre = await _context.QueueParents
                .FirstOrDefaultAsync(x => x.ServerId == datos.ServerId && x.PlanId == datos.PlanId);

            if (datos.ParentCreated && padre == null)
            {
                _context.Add(new QueueParent
                {
                    CorporationId = Convert.ToInt32(user.CorporationId),
                    ParentName = datos.ParentName,
                    ServerId = datos.ServerId,
                    PlanId = datos.PlanId,
                    Up = datos.ParentUp,
                    Down = datos.ParentDown,
                    MkId = datos.ParentMikrotikId
                });
            }
            else if (padre != null)
            {
                padre.Up = datos.ParentUp;
                padre.Down = datos.ParentDown;

                _context.Update(padre);
            }

            var modelo = new ContractQue
            {
                ContractClientId = datos.ContractClientId,
                ServerId = datos.ServerId,
                IpNetId = datos.IpNetId,
                PlanId = datos.PlanId,
                ServerName = datos.ServerName,
                IpServer = datos.IpServer,
                IpCliente = datos.IpCliente,
                PlanName = datos.PlanName,
                TotalVelocidad = datos.TotalVelocidad,
                MikrotikId = datos.MikrotikId
            };

            _context.ContractQues.Add(modelo);

            await ContractAuditLog.AddAsync(_context, modelo.ContractClientId, ContractEventType.QueueCreated,
                modelo.PlanName, $"{user.FirstName} {user.LastName}".Trim(),
                Guid.TryParse(user.Id, out var auditUserId) ? auditUserId : null);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Ok(modelo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractQue>(ex);
        }
    }

    public async Task<ActionResponse<bool>> RemoveQueAsync(ContractQueRemoveDTO datos, string username)
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

            var queue = await _context.ContractQues.FindAsync(datos.ContractQueId);
            if (queue == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            var padre = await _context.QueueParents
                .FirstOrDefaultAsync(x => x.ServerId == queue.ServerId && x.PlanId == queue.PlanId);

            _context.ContractQues.Remove(queue);

            //Si al quitar este cliente el padre se quedo sin nadie, el escritorio ya lo
            //borro del equipo y aqui se borra el registro. Si quedan clientes, solo
            //cambian sus velocidades.
            if (padre != null)
            {
                if (datos.ParentRemoved)
                {
                    _context.Remove(padre);
                }
                else
                {
                    padre.Up = datos.ParentUp;
                    padre.Down = datos.ParentDown;

                    _context.Update(padre);
                }
            }

            await ContractAuditLog.AddAsync(_context, queue.ContractClientId, ContractEventType.QueueCreated,
                $"Queue eliminada: {queue.PlanName}", $"{user.FirstName} {user.LastName}".Trim(),
                Guid.TryParse(user.Id, out var auditUserId) ? auditUserId : null);

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

    // Sirve para crear y para editar: el IpBinding es el unico que se edita sin quitarlo
    public async Task<ActionResponse<ContractBind>> SaveBindAsync(ContractBind modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractBind>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var esNuevo = modelo.ContractBindId == Guid.Empty;

            if (esNuevo)
            {
                var yaExiste = await _context.ContractBinds
                    .AnyAsync(x => x.ContractClientId == modelo.ContractClientId);

                if (yaExiste)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<ContractBind>("Ya existe un IpBinding para este contrato.");
                }

                _context.ContractBinds.Add(modelo);
            }
            else
            {
                //Se busca el que esta y se le copian los campos, NO se adjunta el que llego:
                //el escritorio lo leyo con su CargueDetail incluido y adjuntar ese grafo
                //arrastraria a EF a tocar tablas que aqui no se estan editando
                var data = await _context.ContractBinds.FindAsync(modelo.ContractBindId);
                if (data == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<ContractBind>(_localizer[nameof(Resource.Generic_IdNotFound)]);
                }

                data.ContractClientId = modelo.ContractClientId;
                data.ServerId = modelo.ServerId;
                data.IpNetId = modelo.IpNetId;
                data.CargueDetailId = modelo.CargueDetailId;
                data.HotSpotTypeId = modelo.HotSpotTypeId;
                data.ServerName = modelo.ServerName;
                data.IpServer = modelo.IpServer;
                data.IpCliente = modelo.IpCliente;
                data.MacCliente = modelo.MacCliente;
                data.MikrotikId = modelo.MikrotikId;

                _context.ContractBinds.Update(data);

                modelo = data;
            }

            await ContractAuditLog.AddAsync(_context, modelo.ContractClientId, ContractEventType.BindCreated,
                modelo.MikrotikId, $"{user.FirstName} {user.LastName}".Trim(),
                Guid.TryParse(user.Id, out var auditUserId) ? auditUserId : null);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Ok(modelo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractBind>(ex);
        }
    }

    public async Task<ActionResponse<bool>> RemoveBindAsync(Guid id, string username)
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

            var bind = await _context.ContractBinds.FindAsync(id);
            if (bind == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            _context.ContractBinds.Remove(bind);

            await ContractAuditLog.AddAsync(_context, bind.ContractClientId, ContractEventType.BindCreated,
                $"IpBinding eliminado: {bind.MacCliente}", $"{user.FirstName} {user.LastName}".Trim(),
                Guid.TryParse(user.Id, out var auditUserId) ? auditUserId : null);

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
