using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfaceEntities;
using Spix.UnitOfWork.InterfaceEntities;

namespace Spix.UnitOfWork.ImplementEntities;

public class ManagerUnitOfWork : IManagerUnitOfWork
{
    private readonly IManagerService _managerService;

    public ManagerUnitOfWork(IManagerService managerService)
    {
        _managerService = managerService;
    }

    public async Task<ActionResponse<IEnumerable<Manager>>> GetAsync(PaginationDTO pagination) => await _managerService.GetAsync(pagination);

    public async Task<ActionResponse<Manager>> GetAsync(int id) => await _managerService.GetAsync(id);

    public async Task<ActionResponse<Manager>> UpdateAsync(Manager modelo, string frontUrl) => await _managerService.UpdateAsync(modelo, frontUrl);

    public async Task<ActionResponse<Manager>> AddAsync(Manager modelo, string frontUrl) => await _managerService.AddAsync(modelo, frontUrl);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _managerService.DeleteAsync(id);
}