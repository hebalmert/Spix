﻿using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface IMarkModelUnitOfWork
{
    Task<ActionResponse<IEnumerable<MarkModel>>> ComboAsync(string username, Guid id);

    Task<ActionResponse<IEnumerable<MarkModel>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<MarkModel>> GetAsync(Guid id);

    Task<ActionResponse<MarkModel>> UpdateAsync(MarkModel modelo);

    Task<ActionResponse<MarkModel>> AddAsync(MarkModel modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}