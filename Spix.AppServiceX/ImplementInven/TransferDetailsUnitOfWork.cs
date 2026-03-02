using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class TransferDetailsUnitOfWork : ITransferDetailsUnitOfWork
{
    private readonly ITransferDetailsService _transferDetailsService;

    public TransferDetailsUnitOfWork(ITransferDetailsService transferDetailsService)
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