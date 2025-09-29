using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class RegisterUnitOfWork : IRegisterUnitOfWork
{
    private readonly IRegisterService _registerService;

    public RegisterUnitOfWork(IRegisterService registerService)
    {
        _registerService = registerService;
    }

    public async Task<ActionResponse<IEnumerable<Register>>> GetAsync(PaginationDTO pagination, string username) => await _registerService.GetAsync(pagination, username);

    public async Task<ActionResponse<Register>> GetAsync(Guid id) => await _registerService.GetAsync(id);

    public async Task<ActionResponse<Register>> UpdateAsync(Register modelo) => await _registerService.UpdateAsync(modelo);

    public async Task<ActionResponse<Register>> AddAsync(Register modelo, string username) => await _registerService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _registerService.DeleteAsync(id);
}