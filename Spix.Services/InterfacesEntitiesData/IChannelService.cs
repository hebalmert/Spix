using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesData;

public interface IChannelService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Channel>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Channel>> GetAsync(int id);

    Task<ActionResponse<Channel>> UpdateAsync(Channel modelo);

    Task<ActionResponse<Channel>> AddAsync(Channel modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}