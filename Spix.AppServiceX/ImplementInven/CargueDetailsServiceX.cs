using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class CargueDetailsServiceX : ICargueDetailsServiceX
{
    private readonly ICargueDetailsService _cargueDetailsService;

    public CargueDetailsServiceX(ICargueDetailsService cargueDetailsService)
    {
        _cargueDetailsService = cargueDetailsService;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username, Guid? id = null) => await _cargueDetailsService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetAsync(PaginationDTO pagination, string email) => await _cargueDetailsService.GetAsync(pagination, email);

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetSerialsAsync(PaginationDTO pagination, string email) => await _cargueDetailsService.GetSerialsAsync(pagination, email);

    public async Task<ActionResponse<CargueDetail>> GetAsync(Guid id) => await _cargueDetailsService.GetAsync(id);

    public async Task<ActionResponse<CargueDetail>> UpdateAsync(CargueDetail modelo) => await _cargueDetailsService.UpdateAsync(modelo);

    public async Task<ActionResponse<CargueDetail>> AddAsync(CargueDetail modelo, string email) => await _cargueDetailsService.AddAsync(modelo, email);

    public async Task<ActionResponse<Cargue>> CerrarCargueAsync(Guid id, string email) => await _cargueDetailsService.CerrarCargueAsync(id, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _cargueDetailsService.DeleteAsync(id);
}