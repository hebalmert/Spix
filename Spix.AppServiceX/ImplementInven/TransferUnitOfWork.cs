using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class TransferUnitOfWork : ITransferUnitOfWork
{
    private readonly ITransferService _transferService;

    public TransferUnitOfWork(ITransferService transferService)
    {
        _transferService = transferService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus() => await _transferService.GetComboStatus();

    public async Task<ActionResponse<IEnumerable<Transfer>>> GetAsync(PaginationDTO pagination, string email) => await _transferService.GetAsync(pagination, email);

    public async Task<ActionResponse<Transfer>> GetAsync(Guid id) => await _transferService.GetAsync(id);

    public async Task<ActionResponse<Transfer>> UpdateAsync(Transfer modelo) => await _transferService.UpdateAsync(modelo);

    public async Task<ActionResponse<Transfer>> AddAsync(Transfer modelo, string email) => await _transferService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _transferService.DeleteAsync(id);
}