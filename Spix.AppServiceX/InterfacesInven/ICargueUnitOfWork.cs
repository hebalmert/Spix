using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface ICargueUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<Cargue>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Cargue>> GetAsync(Guid id);

    Task<ActionResponse<Cargue>> UpdateAsync(Cargue modelo);

    Task<ActionResponse<Cargue>> AddAsync(Cargue modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}