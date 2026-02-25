using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class ZoneServiceX : IZoneServiceX
{
    private readonly IZoneService _zoneService;

    public ZoneServiceX(IZoneService zoneService)
    {
        _zoneService = zoneService;
    }

    public async Task<ActionResponse<IEnumerable<Zone>>> ComboAsync(string username, int id) => await _zoneService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<Zone>>> GetAsync(PaginationDTO pagination, string username) => await _zoneService.GetAsync(pagination, username);

    public async Task<ActionResponse<Zone>> GetAsync(Guid id) => await _zoneService.GetAsync(id);

    public async Task<ActionResponse<Zone>> UpdateAsync(Zone modelo) => await _zoneService.UpdateAsync(modelo);

    public async Task<ActionResponse<Zone>> AddAsync(Zone modelo, string username) => await _zoneService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _zoneService.DeleteAsync(id);
}