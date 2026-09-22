using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class CargueBoardServiceX : ICargueBoardServiceX
{
    private readonly ICargueBoardService _cargueBoardService;

    public CargueBoardServiceX(ICargueBoardService cargueBoardService)
    {
        _cargueBoardService = cargueBoardService;
    }

    public async Task<ActionResponse<IEnumerable<CargueListItemDto>>> GetAsync(PaginationDTO pagination, string username) =>
        await _cargueBoardService.GetAsync(pagination, username);

    public async Task<ActionResponse<CargueSummaryDto>> GetSummaryAsync(string username) =>
        await _cargueBoardService.GetSummaryAsync(username);

    public async Task<ActionResponse<CargueProgressDto>> GetProgressAsync(Guid id, string username) =>
        await _cargueBoardService.GetProgressAsync(id, username);

    public async Task<ActionResponse<IEnumerable<CargueSerialDto>>> GetSerialsAsync(Guid id, PaginationDTO pagination, string username) =>
        await _cargueBoardService.GetSerialsAsync(id, pagination, username);
}
