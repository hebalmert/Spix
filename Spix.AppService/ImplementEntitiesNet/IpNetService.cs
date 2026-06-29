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
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using System.Net;

namespace Spix.AppService.ImplementEntitiesNet;

public class IpNetService : IIpNetService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public IpNetService(DataContext context, IHttpContextAccessor httpContextAccessor,
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

    public async Task<ActionResponse<IEnumerable<IpNet>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<IpNet>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            List<IpNet> IpList = new();
            if (id == null)
            {
                IpList = await _context.IpNets
                    .Where(x => x.Active && x.CorporationId == user.CorporationId && x.Assigned == false && x.Excluded == false)
                    .ToListAsync();
                IpList.Insert(0, new IpNet
                {
                    IpNetId = Guid.Empty,
                    Ip = _localizer[nameof(Resource.Select_IP)]
                });
            }
            else
            {
                IpList = await _context.IpNets
                    .Where(x => x.Active && x.CorporationId == user.CorporationId && x.Assigned == false && x.Excluded == false || x.IpNetId == id)
                    .ToListAsync();
            }

            return new ActionResponse<IEnumerable<IpNet>>
            {
                WasSuccess = true,
                Result = IpList
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNet>>(ex); // ✅ Manejo de errores automático
        }
    }


    public async Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<IpNet>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };

            }

            var queryable = _context.IpNets.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Ip!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.Ip).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<IpNet>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNet>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IpNet>> GetAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new ActionResponse<IpNet>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_InvalidId)]
                };
            }

            var modelo = await _context.IpNets.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<IpNet>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<IpNet>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IpNet>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo)
    {
        if (modelo == null || modelo.IpNetId == Guid.Empty)
        {
            return new ActionResponse<IpNet>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.IpNets.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<IpNet>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNet>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<IpNet>
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
                return new ActionResponse<IpNet>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            _context.IpNets.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<IpNet>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNet>(ex); // ✅ Manejo de errores automático
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
            var existingIps = await _context.IpNets
                .Where(x => x.CorporationId == corporationId && requestedIps.Contains(x.Ip!))
                .Select(x => x.Ip!)
                .ToListAsync();

            var existingSet = existingIps.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var ipNets = requestedIps
                .Where(ip => !existingSet.Contains(ip))
                .Select(ip => new IpNet
                {
                    Ip = ip,
                    Active = true,
                    Assigned = false,
                    Excluded = false,
                    CorporationId = corporationId
                })
                .ToList();

            if (ipNets.Count > 0)
            {
                _context.IpNets.AddRange(ipNets);
                await _transactionManager.SaveChangesAsync();
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = ipNets.Count
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
            var ipNets = await _context.IpNets
                .Where(x => x.CorporationId == corporationId
                    && requestedIps.Contains(x.Ip!)
                    && !x.Assigned
                    && !x.Excluded)
                .ToListAsync();

            if (ipNets.Count > 0)
            {
                _context.IpNets.RemoveRange(ipNets);
                await _transactionManager.SaveChangesAsync();
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = ipNets.Count
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
            var DataRemove = await _context.IpNets.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.IpNets.Remove(DataRemove);

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
