using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Spix.AppInfra;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;

namespace Spix.xNetwork.IpHelper;

public class IpNetControl : IIpNetControl
{
    private readonly DataContext _context;

    public IpNetControl(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IpNet>> SelectIpNetWhenAdd(Guid id, string description, IDbContextTransaction transaction)
    {
        try
        {
            var ip = await _context.IpNets.FirstOrDefaultAsync(c => c.IpNetId == id);
            if (ip == null)
                return new ActionResponse<IpNet> { WasSuccess = false, Message = "IP no encontrada" };

            ip.Assigned = true;
            ip.Description = description;

            await _context.SaveChangesAsync();

            return new ActionResponse<IpNet> { WasSuccess = true, Result = ip };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNet> { WasSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ActionResponse<IpNet>> SelectIpNetWhenUpdate(Guid Newid, Guid OldId, string description, IDbContextTransaction transaction)
    {
        try
        {
            if (Newid != Guid.Empty)   //id= IpNetId
            {
                var oldIp = await _context.IpNets.FindAsync(OldId);
                if (oldIp != null)
                {
                    oldIp.Assigned = false; 
                    oldIp.Description = "";
                }

                var newIp = await _context.IpNets.FindAsync(Newid);
                if (newIp != null)
                {
                    newIp.Assigned = true;
                    newIp.Description = description;
                }
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<IpNet> { WasSuccess = true };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNet> { WasSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ActionResponse<IpNet>> SelectIpNetToDelete(Guid id, IDbContextTransaction transaction)
    {
        try
        {
            var ip = await _context.IpNets.FirstOrDefaultAsync(c => c.IpNetId == id);
            if (ip == null)
                return new ActionResponse<IpNet> { WasSuccess = false, Message = "IP no encontrada" };

            ip.Assigned = false;
            ip.Description = "";
            await _context.SaveChangesAsync();

            return new ActionResponse<IpNet> { WasSuccess = true, Result = ip };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<IpNet> { WasSuccess = false, Message = ex.Message };
        }
    }
}
