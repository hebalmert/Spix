using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using System.Net;

namespace Spix.AppService.ImplementEntitiesNet;

public class IpNetworkService : IIpNetworkService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public IpNetworkService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //Combo para nuevo Nodo, donde todas las Ip Libres se deben Mostrar
    public async Task<ActionResponse<IEnumerable<IpNetwork>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<IpNetwork>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            List<IpNetwork> IpList = new();
            if (id == null)
            {
                IpList = await _context.IpNetworks
                    .Where(x => x.Active && x.CorporationId == user.CorporationId && x.Assigned == false && x.Excluded == false)
                    .ToListAsync();
                IpList.Insert(0, new IpNetwork
                {
                    IpNetworkId = Guid.Empty,
                    Ip = _localizer[nameof(Resource.Select_IP)]
                });
            }
            else
            {
                IpList = await _context.IpNetworks
                    .Where(x => x.Active && x.CorporationId == user.CorporationId && x.Assigned == false && x.Excluded == false || x.IpNetworkId == id)
                    .ToListAsync();
            }

            return new ActionResponse<IEnumerable<IpNetwork>>
            {
                WasSuccess = true,
                Result = IpList
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNetwork>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<IpNetwork>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<IpNetwork>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var queryable = _context.IpNetworks.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Ip!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.Ip).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<IpNetwork>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNetwork>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IpNetwork>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<IpNetwork>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.IpNetworks.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<IpNetwork>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            return new ActionResponse<IpNetwork>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IpNetwork>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IpNetwork>> UpdateAsync(IpNetwork modelo)
    {
        if (modelo == null || modelo.IpNetworkId == Guid.Empty)
        {
            return new ActionResponse<IpNetwork>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.IpNetworks.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<IpNetwork>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNetwork>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IpNetwork>> AddAsync(IpNetwork modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<IpNetwork>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IpNetwork>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_InvalidId)]
                };
            }
            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            _context.IpNetworks.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<IpNetwork>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNetwork>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<int>> AddPoolAsync(IpNetPoolCreateDTO modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        var ipBase = modelo.IpAddress?.Trim();
        if (string.IsNullOrWhiteSpace(ipBase) || !IPAddress.TryParse($"{ipBase}.0", out _))
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        if (modelo.Desde > modelo.Hasta)
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        var user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
            };
        }

        var corporationId = Convert.ToInt32(user.CorporationId);
        var requestedIps = Enumerable
            .Range(modelo.Desde, modelo.Hasta - modelo.Desde + 1)
            .Select(number => $"{ipBase}.{number}")
            .ToList();

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var existingIps = await _context.IpNetworks
                .Where(x => x.CorporationId == corporationId && requestedIps.Contains(x.Ip!))
                .Select(x => x.Ip!)
                .ToListAsync();

            var existingSet = existingIps.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var ipNetworks = requestedIps
                .Where(ip => !existingSet.Contains(ip))
                .Select(ip => new IpNetwork
                {
                    Ip = ip,
                    Active = true,
                    Assigned = false,
                    Excluded = false,
                    CorporationId = corporationId
                })
                .ToList();

            if (ipNetworks.Count > 0)
            {
                _context.IpNetworks.AddRange(ipNetworks);
                await _transactionManager.SaveChangesAsync();
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = ipNetworks.Count
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<int>(ex);
        }
    }

    public async Task<ActionResponse<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        var ipBase = modelo.IpAddress?.Trim();
        if (string.IsNullOrWhiteSpace(ipBase) || !IPAddress.TryParse($"{ipBase}.0", out _))
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        if (modelo.Desde > modelo.Hasta)
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        var user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
            };
        }

        var corporationId = Convert.ToInt32(user.CorporationId);
        var requestedIps = Enumerable
            .Range(modelo.Desde, modelo.Hasta - modelo.Desde + 1)
            .Select(number => $"{ipBase}.{number}")
            .ToList();

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var ipNetworks = await _context.IpNetworks
                .Where(x => x.CorporationId == corporationId
                    && requestedIps.Contains(x.Ip!)
                    && !x.Assigned
                    && !x.Excluded)
                .ToListAsync();

            if (ipNetworks.Count > 0)
            {
                _context.IpNetworks.RemoveRange(ipNetworks);
                await _transactionManager.SaveChangesAsync();
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = ipNetworks.Count
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<int>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.IpNetworks.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.IpNetworks.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}
