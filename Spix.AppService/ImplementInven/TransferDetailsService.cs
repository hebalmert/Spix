using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.Services.ImplementInven;

public class TransferDetailsService : ITransferDetailsService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;

    public TransferDetailsService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, IUserHelper userHelper, HttpErrorHandler httpErrorHandle)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;

        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandle;
    }

    public async Task<ActionResponse<IEnumerable<TransferDetails>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<TransferDetails>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.TransferDetails.Where(x => x.CorporationId == user.CorporationId && x.TransferId == pagination.GuidId)
                .Include(x => x.Product)
                .Include(x => x.Product).ThenInclude(x => x!.ProductCategory).AsQueryable();

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.TransferDetailsId).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<TransferDetails>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<TransferDetails>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<TransferDetails>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.TransferDetails.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<TransferDetails>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<TransferDetails>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<TransferDetails>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<TransferDetails>> UpdateAsync(TransferDetails modelo)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.TransferDetails.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<TransferDetails>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<TransferDetails>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<TransferDetails>> AddAsync(TransferDetails modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<TransferDetails>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            //Guardar el nombre del producto para el Historial
            var nombreProduct = await _context.Products.Where(x => x.ProductId == modelo.ProductId)
                .Select(x => x.ProductName).FirstOrDefaultAsync();
            modelo.NameProduct = nombreProduct;

            //Busco el item en TransferDetail, si Existe lo sumo.
            var BuscarItem = await _context.TransferDetails
                .FirstOrDefaultAsync(x => x.TransferId == modelo.TransferId && x.ProductId == modelo.ProductId);
            if (BuscarItem == null)
            {
                _context.TransferDetails.Add(modelo);
            }
            else
            {
                BuscarItem.Quantity += modelo.Quantity;
                _context.TransferDetails.Update(BuscarItem);
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<TransferDetails>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<TransferDetails>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Transfer>> CerrarTransAsync(Transfer modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            //Vemos cuantos Items hay en la compra por PurchaseDetails
            var transferdetails = await _context.TransferDetails.Where(x => x.TransferId == modelo.TransferId).ToListAsync();
            if (transferdetails.Count == 0)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "No Existe ningun Item para poder hacer un Cierre de Transferencia, Agregue Item o Elimine la Transferencia"
                };
            }

            foreach (var item in transferdetails)
            {
                //Vamos Primero a Restar en la Vieja Bodega
                var ProductStockRest = await _context.ProductStocks
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.ProductStorageId == modelo.FromProductStorageId);
                if (ProductStockRest == null)
                {
                    return new ActionResponse<Transfer>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Conseguir el Producto en la Bodega de Origen"
                    };
                }
                else
                {
                    decimal NuevoStock = (decimal)(ProductStockRest.Stock - item.Quantity);
                    ProductStockRest.Stock = NuevoStock;
                    _context.ProductStocks.Update(ProductStockRest);
                }

                //Vamos Primero a Sumar en la nueva Bodega
                var ProductStockPlus = await _context.ProductStocks
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.ProductStorageId == modelo.ToProductStorageId);
                if (ProductStockPlus == null)
                {
                    ProductStock Nuevo = new()
                    {
                        ProductId = item.ProductId,
                        ProductStorageId = modelo.ToProductStorageId,
                        Stock = item.Quantity,
                        CorporationId = item.CorporationId,
                    };
                    _context.ProductStocks.Add(Nuevo);
                }
                else
                {
                    decimal NuevoStock = (decimal)(ProductStockPlus.Stock + item.Quantity);
                    ProductStockPlus.Stock = NuevoStock;
                    _context.ProductStocks.Update(ProductStockPlus);
                }
                await _context.SaveChangesAsync();
            }

            //Cambiamos el estatus del Sells para ya no se pueda editar o borrar.
            var UpdateTrans = await _context.Transfers.FirstOrDefaultAsync(x => x.TransferId == modelo.TransferId);
            if (UpdateTrans == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Error en la Actualizacion del Estado de Venta, no se pudo Guradar Nada"
                };
            }

            UpdateTrans.Status = TransferType.Completado;
            _context.Transfers.Update(UpdateTrans);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Transfer>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Transfer>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.TransferDetails.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            _context.TransferDetails.Remove(DataRemove);

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