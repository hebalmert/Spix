using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractSuspendedServiceX : IContractSuspendedServiceX
{
    private readonly IContractSuspendedService _contractSuspendedService;

    public ContractSuspendedServiceX(IContractSuspendedService contractSuspendedService)
    {
        _contractSuspendedService = contractSuspendedService;
    }

    public async Task<ActionResponse<bool>> SuspendAsync(Guid contractClientId, string? motivo, string username) =>
        await _contractSuspendedService.SuspendAsync(contractClientId, motivo, username);

    public async Task<ActionResponse<SuspendedListDTO>> GetRecordsAsync(string? filter, DateTime? desde, DateTime? hasta, bool soloAbiertas, string username) =>
        await _contractSuspendedService.GetRecordsAsync(filter, desde, hasta, soloAbiertas, username);

    public async Task<ActionResponse<IEnumerable<ActiveContractDTO>>> SearchActiveAsync(string filter, string username) =>
        await _contractSuspendedService.SearchActiveAsync(filter, username);

    public async Task<ActionResponse<IEnumerable<ContractSuspendedDTO>>> SearchAsync(string filter, string username)
    {
        return await _contractSuspendedService.SearchAsync(filter, username);
    }

    public async Task<ActionResponse<ContractSuspendedDTO>> ActivateAsync(Guid id, string username)
    {
        return await _contractSuspendedService.ActivateAsync(id, username);
    }
}
