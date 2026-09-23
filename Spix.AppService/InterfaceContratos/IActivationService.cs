using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceContratos;

public interface IActivationService
{
    //La reactivacion se hace en dos pasos: se revisa y se corre equipo por equipo
    Task<ActionResponse<ActivationCheckDto>> CheckAsync(string username);

    Task<ActionResponse<IEnumerable<ActivationDetailDto>>> GetPendingAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ActivationRunResultDto>> RunServerAsync(Guid serverId, string username);
}
