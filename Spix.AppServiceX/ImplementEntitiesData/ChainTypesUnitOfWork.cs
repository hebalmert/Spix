using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesData;
using Spix.UnitOfWork.InterfacesEntitiesData;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class ChainTypesUnitOfWork : IChainTypesUnitOfWork
{
    private readonly IChainTypesService _chainTypesService;

    public ChainTypesUnitOfWork(IChainTypesService chainTypesService)
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