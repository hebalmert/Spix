using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesData;

public interface IFrecuencyTypeService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<FrecuencyType>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<FrecuencyType>> GetAsync(int id);

    Task<ActionResponse<FrecuencyType>> UpdateAsync(FrecuencyType modelo);

    Task<ActionResponse<FrecuencyType>> AddAsync(FrecuencyType modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}