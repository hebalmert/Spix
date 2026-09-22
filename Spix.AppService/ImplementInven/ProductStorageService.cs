using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementInven;

//Las bodegas de la corporacion. Toda consulta va filtrada por la corporacion del usuario,
//y una bodega con inventario o movimientos no se borra: se inactiva.
public class ProductStorageService : IProductStorageService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;

    public ProductStorageService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //Bodegas activas, con el neutro traducido en la posicion 0
    public async Task<ActionResponse<IEnumerable<ProductStorage>>> ComboAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<ProductStorage>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.ProductStorages
                .AsNoTracking()
                .Where(x => x.Active && x.CorporationId == corporationId)
                .OrderBy(x => x.StorageName)
                .ToListAsync();

            list.Insert(0, new ProductStorage
            {
                ProductStorageId = Guid.Empty,
                StorageName = _localizer["Storage_Select"]
            });

            return Success<IEnumerable<ProductStorage>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ProductStorage>>(ex);
        }
    }

    //El listado: cada bodega con sus existencias contadas en SQL
    public async Task<ActionResponse<IEnumerable<StorageListItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<StorageListItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.ProductStorages
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.StorageName, $"%{filter}%") ||
                    EF.Functions.Like(x.City!.Name, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.StorageName)
                .Paginate(pagination)
                .Select(x => new StorageListItemDto
                {
                    ProductStorageId = x.ProductStorageId,
                    StorageName = x.StorageName,
                    StateName = x.State!.Name,
                    CityName = x.City!.Name,
                    Active = x.Active,
                    Products = x.ProductStocks!.Count(s => s.Stock > 0),
                    Units = x.ProductStocks!.Where(s => s.Stock > 0).Sum(s => (decimal?)s.Stock) ?? 0
                })
                .ToListAsync();

            return Success<IEnumerable<StorageListItemDto>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<StorageListItemDto>>(ex);
        }
    }

    public async Task<ActionResponse<ProductStorage>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<ProductStorage>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.ProductStorages
                .AsNoTracking()
                .Include(x => x.State)
                .ThenInclude(x => x!.Cities)
                .FirstOrDefaultAsync(x => x.ProductStorageId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<ProductStorage>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ProductStorage>(ex);
        }
    }

    public async Task<ActionResponse<ProductStorage>> AddAsync(ProductStorage modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<ProductStorage>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var name = modelo.StorageName.Trim();
            if (await NameExistsAsync(name, corporationId.Value, null)) return await FailRollbackAsync<ProductStorage>(_localizer["Storage_NameRepeated", name]);

            //Lo que decide el servidor
            var nuevo = new ProductStorage
            {
                StorageName = name,
                StateId = modelo.StateId,
                CityId = modelo.CityId,
                Active = modelo.Active,
                CorporationId = corporationId.Value
            };

            //Persistencia
            _context.ProductStorages.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ProductStorage>(ex);
        }
    }

    public async Task<ActionResponse<ProductStorage>> UpdateAsync(ProductStorage modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<ProductStorage>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.ProductStorages.FirstOrDefaultAsync(x =>
                x.ProductStorageId == modelo.ProductStorageId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<ProductStorage>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var name = modelo.StorageName.Trim();
            if (await NameExistsAsync(name, corporationId.Value, current.ProductStorageId)) return await FailRollbackAsync<ProductStorage>(_localizer["Storage_NameRepeated", name]);

            //Mapeo campo por campo
            current.StorageName = name;
            current.StateId = modelo.StateId;
            current.CityId = modelo.CityId;
            current.Active = modelo.Active;

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(current);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ProductStorage>(ex);
        }
    }

    //Solo se borra una bodega que nunca se uso: sin existencias, compras ni traslados
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.ProductStorages.FirstOrDefaultAsync(x =>
                x.ProductStorageId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var inUse = await _context.ProductStocks.AnyAsync(x => x.ProductStorageId == id) ||
                        await _context.Purchases.AnyAsync(x => x.ProductStorageId == id) ||
                        await _context.Transfers.AnyAsync(x => x.FromProductStorageId == id || x.ToProductStorageId == id);
            if (inUse) return await FailRollbackAsync<bool>(_localizer["Storage_InUse", current.StorageName]);

            //Persistencia
            _context.ProductStorages.Remove(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    private async Task<bool> NameExistsAsync(string name, int corporationId, Guid? productStorageId)
    {
        return await _context.ProductStorages.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.StorageName == name &&
            x.ProductStorageId != productStorageId);
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fail<T>(message);
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
