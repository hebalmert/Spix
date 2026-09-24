using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesInven;

public interface IReportInventoryServiceX
{
    Task<ActionResponse<ReportSerialSummaryDto>> GetSerialSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ReportSerialDto>>> GetSerialsAsync(string username);
}
