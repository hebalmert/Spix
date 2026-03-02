using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesData;

public interface IFrecuencyTypeService
{
    Task<ActionResponse<IEnumerable<FrecuencyType>>> ComboAsync();

    Task<ActionResponse<IEnumerable<FrecuencyType>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<FrecuencyType>> GetAsync(int id);

    Task<ActionResponse<FrecuencyType>> UpdateAsync(FrecuencyType modelo);

    Task<ActionResponse<FrecuencyType>> AddAsync(FrecuencyType modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}