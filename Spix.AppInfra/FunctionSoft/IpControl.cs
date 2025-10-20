using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Spix.Core.EntitiesNet;
using Spix.DomainLogic.SpixResponse;

namespace Spix.AppInfra.FunctionSoft;

public class IpControl : IIpControl
{
    private readonly DataContext _context;

    public IpControl(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IpNetwork>> SelectIpWhenAdd(Guid id, string Descrip, IDbContextTransaction transaction)
    {
        var ip = await _context.IpNetworks.FirstOrDefaultAsync(c => c.IpNetworkId == id);
        if (ip == null)
        {
            return new ActionResponse<IpNetwork> { WasSuccess = false, Message = "IP no encontrada" };
        }

        ip!.Assigned = true;
        ip.Description = Descrip;
        _context.Update(ip);
        await _context.SaveChangesAsync();

        return new ActionResponse<IpNetwork>
        {
            WasSuccess = true
        };
    }

    //id=IpNetwork   IdNode = modelo.NodeId para verificar si hubo cambio de IP
    public async Task<ActionResponse<IpNetwork>> SelectIpWhenUpdate(Guid id, Guid IdNode, string Descrip, IDbContextTransaction transaction)
    {
        var CurrentIpNetwork = await _context.Nodes.AsNoTracking().FirstOrDefaultAsync(x => x.NodeId == IdNode);
        if (CurrentIpNetwork!.IpNetworkId != id)
        {
            var currenIp = await _context.IpNetworks.FindAsync(CurrentIpNetwork.IpNetworkId);
            currenIp!.Assigned = false;
            currenIp.Description = "";
            _context.Update(currenIp);

            var upIp = await _context.IpNetworks.FindAsync(id);
            upIp!.Assigned = true;
            upIp.Description = Descrip;
            _context.Update(upIp);
        }
        await _context.SaveChangesAsync();

        return new ActionResponse<IpNetwork>
        {
            WasSuccess = true
        };
    }

    //id=IpNetwork   IdNode = modelo.NodeId para verificar si hubo cambio de IP
    public async Task<ActionResponse<IpNetwork>> SelectIpWhenUpdateServer(Guid id, Guid IdServer, string Descrip, IDbContextTransaction transaction)
    {
        var CurrentIpNetwork = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(x => x.ServerId == IdServer);
        if (CurrentIpNetwork!.IpNetworkId != id)
        {
            var currenIp = await _context.IpNetworks.FindAsync(CurrentIpNetwork.IpNetworkId);
            currenIp!.Assigned = false;
            currenIp.Description = "";
            _context.Update(currenIp);

            var upIp = await _context.IpNetworks.FindAsync(id);
            upIp!.Assigned = true;
            upIp.Description = Descrip;
            _context.Update(upIp);
        }
        await _context.SaveChangesAsync();

        return new ActionResponse<IpNetwork>
        {
            WasSuccess = true
        };
    }

    public async Task<ActionResponse<IpNetwork>> SelectIpToDelete(Guid id, IDbContextTransaction transaction)
    {
        var ip = await _context.IpNetworks.FirstOrDefaultAsync(c => c.IpNetworkId == id);
        if (ip == null)
        {
            return new ActionResponse<IpNetwork> { WasSuccess = false, Message = "IP no encontrada" };
        }
        ip!.Assigned = false;
        ip.Description = "";
        _context.Update(ip);
        await _context.SaveChangesAsync();

        return new ActionResponse<IpNetwork>
        {
            WasSuccess = true
        };
    }
}