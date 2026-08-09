using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesGen;

public interface IEstratoSocialService
{
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<EstratoSocial>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<EstratoSocial>> GetAsync(Guid id);

    Task<ActionResponse<EstratoSocial>> UpdateAsync(EstratoSocial modelo);

    Task<ActionResponse<EstratoSocial>> AddAsync(EstratoSocial modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
