using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractExemptServiceX
{
    Task<ActionResponse<bool>> ExemptAsync(Guid contractClientId, string? motivo, string username);

    Task<ActionResponse<ExemptListDTO>> GetRecordsAsync(string? filter, DateTime? desde, DateTime? hasta, bool soloAbiertas, string username);

    Task<ActionResponse<IEnumerable<ActiveContractDTO>>> SearchActiveAsync(string filter, string username);

    Task<ActionResponse<bool>> ActivateAsync(Guid contractClientId, string username);
}
