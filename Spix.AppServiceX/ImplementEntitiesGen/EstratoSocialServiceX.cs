using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class EstratoSocialServiceX : IEstratoSocialServiceX
{
    private readonly IEstratoSocialService _estratoSocialService;

    public EstratoSocialServiceX(IEstratoSocialService estratoSocialService)
    {
        _estratoSocialService = estratoSocialService;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username)
    {
        return await _estratoSocialService.ComboAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<EstratoSocial>>> GetAsync(PaginationDTO pagination, string username)
    {
        return await _estratoSocialService.GetAsync(pagination, username);
    }

    public async Task<ActionResponse<EstratoSocial>> GetAsync(Guid id)
    {
        return await _estratoSocialService.GetAsync(id);
    }

    public async Task<ActionResponse<EstratoSocial>> UpdateAsync(EstratoSocial modelo)
    {
        return await _estratoSocialService.UpdateAsync(modelo);
    }

    public async Task<ActionResponse<EstratoSocial>> AddAsync(EstratoSocial modelo, string username)
    {
        return await _estratoSocialService.AddAsync(modelo, username);
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        return await _estratoSocialService.DeleteAsync(id);
    }
}
