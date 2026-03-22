using Spix.AppService.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class ChainTypesServiceX : IChainTypesServiceX
{
    private readonly IChainTypesService _chainTypesService;

    public ChainTypesServiceX(IChainTypesService chainTypesService)
    {
        _chainTypesService = chainTypesService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync() => await _chainTypesService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<ChainType>>> GetAsync(PaginationDTO pagination) => await _chainTypesService.GetAsync(pagination);

    public async Task<ActionResponse<ChainType>> GetAsync(int id) => await _chainTypesService.GetAsync(id);

    public async Task<ActionResponse<ChainType>> UpdateAsync(ChainType modelo) => await _chainTypesService.UpdateAsync(modelo);

    public async Task<ActionResponse<ChainType>> AddAsync(ChainType modelo) => await _chainTypesService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _chainTypesService.DeleteAsync(id);
}