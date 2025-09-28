using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesData;
using Spix.UnitOfWork.InterfacesEntitiesData;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class OperationUnitOfWork : IOperationUnitOfWork
{
    private readonly IOperationService _operationService;

    public OperationUnitOfWork(IOperationService operationService)
    {
        _operationService = operationService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync() => await _operationService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<Operation>>> GetAsync(PaginationDTO pagination) => await _operationService.GetAsync(pagination);

    public async Task<ActionResponse<Operation>> GetAsync(int id) => await _operationService.GetAsync(id);

    public async Task<ActionResponse<Operation>> UpdateAsync(Operation modelo) => await _operationService.UpdateAsync(modelo);

    public async Task<ActionResponse<Operation>> AddAsync(Operation modelo) => await _operationService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _operationService.DeleteAsync(id);
}