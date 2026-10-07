using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class TransferServiceX : ITransferServiceX
{
    private readonly ITransferService _transferService;

    public TransferServiceX(ITransferService transferService)
    {
        _transferService = transferService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus() => await _transferService.GetComboStatus();

    public async Task<ActionResponse<IEnumerable<TextItemModel>>> ReceiversComboAsync(string username) => await _transferService.ReceiversComboAsync(username);

    public async Task<ActionResponse<IEnumerable<Transfer>>> GetAsync(PaginationDTO pagination, string email) => await _transferService.GetAsync(pagination, email);

    public async Task<ActionResponse<Transfer>> GetAsync(Guid id, string username) => await _transferService.GetAsync(id, username);

    public async Task<ActionResponse<Transfer>> UpdateAsync(Transfer modelo, string username) => await _transferService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<Transfer>> AddAsync(Transfer modelo, string email) => await _transferService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _transferService.DeleteAsync(id, username);
}