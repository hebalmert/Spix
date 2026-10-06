using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class TransferDetailsServiceX : ITransferDetailsServiceX
{
    private readonly ITransferDetailsService _transferDetailsService;

    public TransferDetailsServiceX(ITransferDetailsService transferDetailsService)
    {
        _transferDetailsService = transferDetailsService;
    }

    public async Task<ActionResponse<IEnumerable<TransferDetails>>> GetAsync(PaginationDTO pagination, string email) => await _transferDetailsService.GetAsync(pagination, email);

    public async Task<ActionResponse<TransferDetails>> GetAsync(Guid id, string username) => await _transferDetailsService.GetAsync(id, username);

    public async Task<ActionResponse<TransferDetails>> UpdateAsync(TransferDetails modelo, string username) => await _transferDetailsService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<TransferDetails>> AddAsync(TransferDetails modelo, string email) => await _transferDetailsService.AddAsync(modelo, email);

    public async Task<ActionResponse<Transfer>> CerrarTransAsync(Transfer modelo, string email) => await _transferDetailsService.CerrarTransAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _transferDetailsService.DeleteAsync(id, username);
}