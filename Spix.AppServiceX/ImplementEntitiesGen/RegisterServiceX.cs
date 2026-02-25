using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class RegisterServiceX : IRegisterServiceX
{
    private readonly IRegisterService _registerService;

    public RegisterServiceX(IRegisterService registerService)
    {
        _registerService = registerService;
    }

    public async Task<ActionResponse<IEnumerable<Register>>> GetAsync(PaginationDTO pagination, string username) => await _registerService.GetAsync(pagination, username);

    public async Task<ActionResponse<Register>> GetAsync(Guid id) => await _registerService.GetAsync(id);

    public async Task<ActionResponse<Register>> UpdateAsync(Register modelo) => await _registerService.UpdateAsync(modelo);

    public async Task<ActionResponse<Register>> AddAsync(Register modelo, string username) => await _registerService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _registerService.DeleteAsync(id);
}