using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesInven;

public interface IReportInventoryServiceX
{
    Task<ActionResponse<ReportSerialSummaryDto>> GetSerialSummaryAsync(string username);

    ActionResponse<IEnumerable<IntItemModel>> SerialStatesCombo();

    Task<ActionResponse<IEnumerable<ReportSerialDetailDto>>> GetSerialDetailAsync(string username, SerialStateType? estado, Guid? productId, Guid? storageId);

    Task<ActionResponse<IEnumerable<ReportSerialDto>>> GetSerialsAsync(string username);
}
