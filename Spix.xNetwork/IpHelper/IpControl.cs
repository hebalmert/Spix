using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;

namespace Spix.xNetwork.IpHelper;

public class IpControl : IIpControl
{
    private readonly DataContext _context;

    public IpControl(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> AssignAsync(Guid ipNetworkId, Guid? previousIpNetworkId, string description, int corporationId)
    {
        var ip = await _context.IpNetworks.FirstOrDefaultAsync(x =>
            x.IpNetworkId == ipNetworkId &&
            x.CorporationId == corporationId);
        if (ip == null) return false;

        //La misma IP de antes: solo se actualiza el nombre del equipo
        if (ipNetworkId == previousIpNetworkId)
        {
            ip.Description = description;
            return true;
        }

        //Una IP nueva tiene que estar libre para usarse
        if (!ip.Active || ip.Excluded || ip.Assigned) return false;

        if (previousIpNetworkId != null)
        {
            await ReleaseAsync(previousIpNetworkId.Value, corporationId);
        }

        ip.Assigned = true;
        ip.Description = description;
        return true;
    }

    public async Task ReleaseAsync(Guid ipNetworkId, int corporationId)
    {
        var ip = await _context.IpNetworks.FirstOrDefaultAsync(x =>
            x.IpNetworkId == ipNetworkId &&
            x.CorporationId == corporationId);
        if (ip == null) return;

        ip.Assigned = false;
        ip.Description = string.Empty;
    }
}
