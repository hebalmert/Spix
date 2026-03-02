using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface IChannelUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Channel>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Channel>> GetAsync(int id);

    Task<ActionResponse<Channel>> UpdateAsync(Channel modelo);

    Task<ActionResponse<Channel>> AddAsync(Channel modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}