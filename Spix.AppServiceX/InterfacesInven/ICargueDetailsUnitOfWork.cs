using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface ICargueDetailsUnitOfWork
{
    Task<ActionResponse<IEnumerable<CargueDetail>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<IEnumerable<CargueDetail>>> GetSerialsAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<CargueDetail>> GetAsync(Guid id);

    Task<ActionResponse<CargueDetail>> UpdateAsync(CargueDetail modelo);

    Task<ActionResponse<CargueDetail>> AddAsync(CargueDetail modelo, string email);

    Task<ActionResponse<Cargue>> CerrarCargueAsync(Guid id, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}