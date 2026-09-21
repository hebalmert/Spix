using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractExemptServiceX : IContractExemptServiceX
{
    private readonly IContractExemptService _contractExemptService;

    public ContractExemptServiceX(IContractExemptService contractExemptService)
    {
        _contractExemptService = contractExemptService;
    }

    public async Task<ActionResponse<bool>> ExemptAsync(Guid contractClientId, string? motivo, string username) =>
        await _contractExemptService.ExemptAsync(contractClientId, motivo, username);

    public async Task<ActionResponse<ExemptListDTO>> GetRecordsAsync(string? filter, DateTime? desde, DateTime? hasta, bool soloAbiertas, string username) =>
        await _contractExemptService.GetRecordsAsync(filter, desde, hasta, soloAbiertas, username);

    public async Task<ActionResponse<IEnumerable<ActiveContractDTO>>> SearchActiveAsync(string filter, string username) =>
        await _contractExemptService.SearchActiveAsync(filter, username);

    public async Task<ActionResponse<bool>> ActivateAsync(Guid contractClientId, string username) =>
        await _contractExemptService.ActivateAsync(contractClientId, username);
}
