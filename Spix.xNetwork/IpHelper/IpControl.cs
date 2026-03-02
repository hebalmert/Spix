using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Spix.AppInfra;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;

namespace Spix.xNetwork.IpHelper;

public class IpControl : IIpControl
{
    private readonly DataContext _context;

    public IpControl(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IpNetwork>> SelectIpWhenAdd(Guid id, string description, IDbContextTransaction transaction)
    {
        try
        {
            var ip = await _context.IpNetworks.FirstOrDefaultAsync(c => c.IpNetworkId == id);
            if (ip == null)
                return new ActionResponse<IpNetwork> { WasSuccess = false, Message = "IP no encontrada" };

            ip.Assigned = true;
            ip.Description = description;

            _context.Update(ip);
            await _context.SaveChangesAsync();

            return new ActionResponse<IpNetwork> { WasSuccess = true, Result = ip };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNetwork> { WasSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ActionResponse<IpNetwork>> SelectIpWhenUpdate(Guid id, Guid entityId, string description, IDbContextTransaction transaction)
    {
        try
        {
            var node = await _context.Nodes.AsNoTracking().FirstOrDefaultAsync(x => x.NodeId == entityId);
            if (node == null)
                return new ActionResponse<IpNetwork> { WasSuccess = false, Message = "Nodo no encontrado" };

            if (node.IpNetworkId != id)
            {
                var oldIp = await _context.IpNetworks.FindAsync(node.IpNetworkId);
                if (oldIp != null)
                {
                    oldIp.Assigned = false;
                    oldIp.Description = "";
                    _context.Update(oldIp);
                }

                var newIp = await _context.IpNetworks.FindAsync(id);
                if (newIp != null)
                {
                    newIp.Assigned = true;
                    newIp.Description = description;
                    _context.Update(newIp);
                }
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<IpNetwork> { WasSuccess = true };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNetwork> { WasSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ActionResponse<IpNetwork>> SelectIpToDelete(Guid id, IDbContextTransaction transaction)
    {
        try
        {
            var ip = await _context.IpNetworks.FirstOrDefaultAsync(c => c.IpNetworkId == id);
            if (ip == null)
                return new ActionResponse<IpNetwork> { WasSuccess = false, Message = "IP no encontrada" };

            ip.Assigned = false;
            ip.Description = "";

            _context.Update(ip);
            await _context.SaveChangesAsync();

            return new ActionResponse<IpNetwork> { WasSuccess = true, Result = ip };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNetwork> { WasSuccess = false, Message = ex.Message };
        }
    }


    public async Task<ActionResponse<IpNetwork>> SelectIpWhenUpdateServer(Guid id, Guid IdServer, string Descrip, IDbContextTransaction transaction)
    {
        try
        {
            var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(x => x.ServerId == IdServer);
            if (server == null)
                return new ActionResponse<IpNetwork> { WasSuccess = false, Message = "Servidor no encontrado" };

            if (server.IpNetworkId != id)
            {
                var oldIp = await _context.IpNetworks.FindAsync(server.IpNetworkId);
                if (oldIp != null)
                {
                    oldIp.Assigned = false;
                    oldIp.Description = "";
                    _context.Update(oldIp);
                }

                var newIp = await _context.IpNetworks.FindAsync(id);
                if (newIp != null)
                {
                    newIp.Assigned = true;
                    newIp.Description = Descrip;
                    _context.Update(newIp);
                }
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<IpNetwork> { WasSuccess = true };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNetwork> { WasSuccess = false, Message = ex.Message };
        }

    }
}
