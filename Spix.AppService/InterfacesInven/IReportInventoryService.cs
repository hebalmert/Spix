using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesInven;

public interface IReportInventoryService
{
    Task<ActionResponse<ReportSerialSummaryDto>> GetSerialSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ReportSerialDto>>> GetSerialsAsync(string username);
}
