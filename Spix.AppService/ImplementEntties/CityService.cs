using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceEntities;
using Spix.Domain.Entities;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.Services.ImplementEntties;

public class CityService : ICityService
{
    private readonly DataContext _context;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IStringLocalizer _localizer;

    public CityService(DataContext context, HttpErrorHandler httpErrorHandler,
        IHttpContextAccessor httpContextAccessor, ITransactionManager transactionManager,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandler;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<City>>> ComboAsync(int id)
    {
        try
        {
            IEnumerable<City> ListModel = await _context.Cities.Where(x => x.StateId == id).ToListAsync();
            return new ActionResponse<IEnumerable<City>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<City>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<City>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            //pagination.Id == Trae el ID del Country
            var queryable = _context.Cities.Where(x => x.StateId == pagination.Id).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                //Busqueda grandes mateniendo los indices de los campos, campo Esta Collation CI para Case Insensitive
                queryable = queryable.Where(u => EF.Functions.Like(u.Name, $"%{pagination.Filter}%"));
            }
            var result = await queryable.ApplyFullPaginationAsync(_httpContextAccessor.HttpContext!, pagination);

            return new ActionResponse<IEnumerable<City>>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<City>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<City>> GetAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ActionResponse<City>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_InvalidId"]
                };
            }
            var modelo = await _context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CityId == id);
            if (modelo == null)
            {
                return new ActionResponse<City>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }
            return new ActionResponse<City>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<City>(ex);
        }
    }

    public async Task<ActionResponse<City>> UpdateAsync(City modelo)
    {
        if (modelo == null || modelo.CityId <= 0)
        {
            return new ActionResponse<City>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidId"]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Cities.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<City>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer["Generic_Success"]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<City>(ex);
        }
    }

    public async Task<ActionResponse<City>> AddAsync(City modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<City>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidModel"] // 🧠 Clave multilenguaje para modelo nulo
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Cities.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<City>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer["Generic_Success"] // 🌐 Mensaje localizado de éxito
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<City>(ex); // ✅ Multilenguaje automático en errores
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidId"] // 🌐 Localizado para ID inválido
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Cities.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"] // 🌐 Localizado para no encontrado
                };
            }

            _context.Cities.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = _localizer["Generic_Success"] // 🌐 Localizado para éxito
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}