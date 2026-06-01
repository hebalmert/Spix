using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementInven;

public class CargueDetailsService : ICargueDetailsService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public CargueDetailsService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, IUserHelper userHelper, HttpErrorHandler httpErrorHandle,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;

        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandle;
        _localizer = localizer;
    }
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<GuidItemModel>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            List<GuidItemModel> ListMac = new();
            if (id == null)
            {
                ListMac = await _context.CargueDetails
                    .Where(x => x.CorporationId == user.CorporationId && x.Status == SerialStateType.Disponible)
                    .Select(x => new GuidItemModel
                    {
                        Value = x.CargueDetailId,
                        Name = x.MacWlan
                    })
                    .ToListAsync();
                ListMac.Insert(0, new GuidItemModel
                {
                    Value = Guid.Empty,
                    Name = _localizer[nameof(Resource.Select_IP)]
                });
            }
            else
            {
                ListMac = await _context.CargueDetails
                    .Where(x => x.CorporationId == user.CorporationId && x.Status == SerialStateType.Disponible || x.CargueDetailId == id)
                                        .Select(x => new GuidItemModel
                                        {
                                            Value = x.CargueDetailId,
                                            Name = x.MacWlan
                                        })
                    .ToListAsync();
            }

            return new ActionResponse<IEnumerable<GuidItemModel>>
            {
                WasSuccess = true,
                Result = ListMac
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<CargueDetail>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.CargueDetails
                .Where(x => x.CorporationId == user.CorporationId && x.CargueId == pagination.GuidId)
                .Include(x => x.Cargue).AsQueryable();

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.DateCargue).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<CargueDetail>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CargueDetail>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetSerialsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<CargueDetail>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.CargueDetails.Where(x => x.CorporationId == user.CorporationId)
                .Include(x => x.Cargue).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.MacWlan!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.DateCargue).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<CargueDetail>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CargueDetail>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<CargueDetail>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.CargueDetails
                .Include(x => x.Cargue).ThenInclude(x => x!.Product)
                .FirstOrDefaultAsync(x => x.CargueDetailId == id);
            if (modelo == null)
            {
                return new ActionResponse<CargueDetail>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<CargueDetail>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CargueDetail>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<CargueDetail>> UpdateAsync(CargueDetail modelo)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            CargueDetail NewModelo = _mapperService.Map<CargueDetail, CargueDetail>(modelo);

            _context.CargueDetails.Update(NewModelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<CargueDetail>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CargueDetail>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<CargueDetail>> AddAsync(CargueDetail modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<CargueDetail>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            modelo.DateCargue = DateTime.Now;

            _context.CargueDetails.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<CargueDetail>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CargueDetail>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Cargue>> CerrarCargueAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Cargue>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            //Cambiamos el estatus del Sells para ya no se pueda editar o borrar.
            var UpdateCargue = await _context.Cargues.FirstOrDefaultAsync(x => x.CargueId == id && x.CorporationId == user.CorporationId);
            if (UpdateCargue == null)
            {
                return new ActionResponse<Cargue>
                {
                    WasSuccess = false,
                    Message = "Error en la Actualizacion del Estado de Venta, no se pudo Guradar Nada"
                };
            }

            UpdateCargue.Status = CargueType.Completado;
            _context.Cargues.Update(UpdateCargue);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Cargue>
            {
                WasSuccess = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Cargue>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.CargueDetails.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            _context.CargueDetails.Remove(DataRemove);

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