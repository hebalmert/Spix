using Spix.AppService.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class FrecuencyServiceX : IFrecuencyServiceX
{
    private readonly IFrecuencyService _frecuencyService;

    public FrecuencyServiceX(IFrecuencyService frecuencyService)
    {
        _frecuencyService = frecuencyService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync(int id) => await _frecuencyService.ComboAsync(id);

    public async Task<ActionResponse<IEnumerable<Frecuency>>> GetAsync(PaginationDTO pagination) => await _frecuencyService.GetAsync(pagination);

    public async Task<ActionResponse<Frecuency>> GetAsync(int id) => await _frecuencyService.GetAsync(id);

    public async Task<ActionResponse<Frecuency>> UpdateAsync(Frecuency modelo) => await _frecuencyService.UpdateAsync(modelo);

    public async Task<ActionResponse<Frecuency>> AddAsync(Frecuency modelo) => await _frecuencyService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _frecuencyService.DeleteAsync(id);
}