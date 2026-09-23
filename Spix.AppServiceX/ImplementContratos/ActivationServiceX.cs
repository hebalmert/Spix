using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementContratos;

public class ActivationServiceX : IActivationServiceX
{
    private readonly IActivationService _activationService;

    public ActivationServiceX(IActivationService activationService)
    {
        _activationService = activationService;
    }

    public async Task<ActionResponse<ActivationCheckDto>> CheckAsync(string username)
    {
        return await _activationService.CheckAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<ActivationDetailDto>>> GetPendingAsync(PaginationDTO pagination, string username)
    {
        return await _activationService.GetPendingAsync(pagination, username);
    }

    public async Task<ActionResponse<ActivationRunResultDto>> RunServerAsync(Guid serverId, string username)
    {
        return await _activationService.RunServerAsync(serverId, username);
    }
}
