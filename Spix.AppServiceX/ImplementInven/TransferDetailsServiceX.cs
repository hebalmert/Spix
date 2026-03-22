using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class TransferDetailsServiceX : ITransferDetailsServiceX
{
    private readonly ITransferDetailsServiceX _transferDetailsService;

    public TransferDetailsServiceX(ITransferDetailsServiceX transferDetailsService)
    {
        _transferDetailsService = transferDetailsService;
    }

    public async Task<ActionResponse<IEnumerable<TransferDetails>>> GetAsync(PaginationDTO pagination, string email) => await _transferDetailsService.GetAsync(pagination, email);

    public async Task<ActionResponse<TransferDetails>> GetAsync(Guid id) => await _transferDetailsService.GetAsync(id);

    public async Task<ActionResponse<TransferDetails>> UpdateAsync(TransferDetails modelo) => await _transferDetailsService.UpdateAsync(modelo);

    public async Task<ActionResponse<TransferDetails>> AddAsync(TransferDetails modelo, string email) => await _transferDetailsService.AddAsync(modelo, email);

    public async Task<ActionResponse<Transfer>> CerrarTransAsync(Transfer modelo, string email) => await _transferDetailsService.CerrarTransAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _transferDetailsService.DeleteAsync(id);
}