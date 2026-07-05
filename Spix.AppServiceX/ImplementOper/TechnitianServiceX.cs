using Spix.AppService.InterfacesOper;
using Spix.AppServiceX.InterfacesOper;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.ImplementOper;

public class TechnitianServiceX : ITechnitianServiceX
{
    private readonly ITechnitianService _technitianService;

    public TechnitianServiceX(ITechnitianService technitianService)
    {
        _technitianService = technitianService;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username) => await _technitianService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<Technician>>> GetAsync(PaginationDTO pagination, string username) => await _technitianService.GetAsync(pagination, username);

    public async Task<ActionResponse<Technician>> GetAsync(Guid id) => await _technitianService.GetAsync(id);

    public async Task<ActionResponse<Technician>> UpdateAsync(Technician modelo, string frontUrl) => await _technitianService.UpdateAsync(modelo, frontUrl);

    public async Task<ActionResponse<Technician>> AddAsync(Technician modelo, string username, string frontUrl) => await _technitianService.AddAsync(modelo, username, frontUrl);

    public async Task<ActionResponse<bool>> ResendActivationEmailAsync(Guid id, string frontUrl) => await _technitianService.ResendActivationEmailAsync(id, frontUrl);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _technitianService.DeleteAsync(id);
}
