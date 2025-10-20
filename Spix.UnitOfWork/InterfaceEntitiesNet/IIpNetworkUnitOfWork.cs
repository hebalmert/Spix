﻿using Spix.Core.EntitiesNet;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfaceEntitiesNet;

public interface IIpNetworkUnitOfWork
{
    Task<ActionResponse<IEnumerable<IpNetwork>>> ComboAsync(string email, Guid? id = null);

    Task<ActionResponse<IEnumerable<IpNetwork>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<IpNetwork>> GetAsync(Guid id);

    Task<ActionResponse<IpNetwork>> UpdateAsync(IpNetwork modelo);

    Task<ActionResponse<IpNetwork>> AddAsync(IpNetwork modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}