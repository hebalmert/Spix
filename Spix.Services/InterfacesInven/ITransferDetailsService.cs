﻿using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesInven;

public interface ITransferDetailsService
{
    Task<ActionResponse<IEnumerable<TransferDetails>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<TransferDetails>> GetAsync(Guid id);

    Task<ActionResponse<TransferDetails>> UpdateAsync(TransferDetails modelo);

    Task<ActionResponse<TransferDetails>> AddAsync(TransferDetails modelo, string email);

    Task<ActionResponse<Transfer>> CerrarTransAsync(Transfer modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}