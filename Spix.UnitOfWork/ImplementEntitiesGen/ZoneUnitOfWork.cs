using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class ZoneUnitOfWork : IZoneUnitOfWork
{
    private readonly IZoneService _zoneService;

    public ZoneUnitOfWork(IZoneService zoneService)
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