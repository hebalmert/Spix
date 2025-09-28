﻿using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesData;

public interface IOperationService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Operation>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Operation>> GetAsync(int id);

    Task<ActionResponse<Operation>> UpdateAsync(Operation modelo);

    Task<ActionResponse<Operation>> AddAsync(Operation modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}