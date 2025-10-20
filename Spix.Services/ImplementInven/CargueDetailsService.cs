using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;

namespace Spix.Services.ImplementInven;

public class CargueDetailsService : ICargueDetailsService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;

    public CargueDetailsService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, IUserHelper userHelper, HttpErrorHandler httpErrorHandle)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;

        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandle;
    }

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetAsync(PaginationDTO pagination, string email)
    {
        try
        {
            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<CargueDetail>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.CargueDetails.Where(x => x.CorporationId == user.CorporationId && x.CargueId == pagination.GuidId)
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

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetSerialsAsync(PaginationDTO pagination, string email)
    {
        try
        {
            var user = await _userHelper.GetUserAsync(email);
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
                .Include(x => x.Cargue)
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

    public async Task<ActionResponse<CargueDetail>> AddAsync(CargueDetail modelo, string email)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserAsync(email);
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

    public async Task<ActionResponse<Cargue>> CerrarCargueAsync(Guid id, string email)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserAsync(email);
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