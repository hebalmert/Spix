using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IActivationServiceX
{
    Task<ActionResponse<ActivationCheckDto>> CheckAsync(string username);

    Task<ActionResponse<IEnumerable<ActivationDetailDto>>> GetPendingAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ActivationRunResultDto>> RunServerAsync(Guid serverId, string username);
}
