﻿using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface IMarkUnitOfWork
{
    Task<ActionResponse<IEnumerable<Mark>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Mark>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Mark>> GetAsync(Guid id);

    Task<ActionResponse<Mark>> UpdateAsync(Mark modelo);

    Task<ActionResponse<Mark>> AddAsync(Mark modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}