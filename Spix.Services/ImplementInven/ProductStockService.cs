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
using Spix.DomainLogic.EntitiesDTO;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;

namespace Spix.Services.ImplementInven;

public class ProductStockService : IProductStockService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IUserHelper _userHelper;

    public ProductStockService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache,
        IUserHelper userHelper, HttpErrorHandler httpErrorHandle)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandle;
    }

    public async Task<ActionResponse<IEnumerable<ProductStock>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<ProductStock>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.ProductStocks
                .Include(x => x.ProductStorage).Include(x => x.Product)
                .Where(x => x.CorporationId == user.CorporationId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.ProductStorage!.StorageName!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.ProductStorage!.StorageName!).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<ProductStock>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ProductStock>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<ProductStock>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.ProductStocks
                .FirstOrDefaultAsync(x => x.ProductStockId == id);
            if (modelo == null)
            {
                return new ActionResponse<ProductStock>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<ProductStock>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ProductStock>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<TransferStockDTO>> GetProductStock(TransferStockDTO modelo)
    {
        try
        {
            var bodegaOrigen = await _context.Transfers.FindAsync(modelo.TransferId);
            var stockDisponible = await _context.ProductStocks
                .FirstOrDefaultAsync(x => x.ProductId == modelo.ProductId && x.ProductStorageId == bodegaOrigen!.FromProductStorageId);
            if (stockDisponible == null || stockDisponible.Stock == 0)
            {
                return new ActionResponse<TransferStockDTO>
                {
                    WasSuccess = false,
                    Message = "No Existe Este Producto en esta Bodega o El inventario es igual a 0 (Cero)"
                };
            }
            TransferStockDTO NStock = new()
            {
                ProductId = modelo.ProductId,
                TransferId = modelo.TransferId,
                DiponibleOrigen = stockDisponible!.Stock
            };

            if (modelo == null)
            {
                return new ActionResponse<TransferStockDTO>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<TransferStockDTO>
            {
                WasSuccess = true,
                Result = NStock
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<TransferStockDTO>(ex); // ✅ Manejo de errores automático
        }
    }
}