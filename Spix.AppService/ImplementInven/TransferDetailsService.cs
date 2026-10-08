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
using Spix.DomainLogic.ItemsGeneric;
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

            var queryable = _context.TransferDetails.AsNoTracking().Where(x => x.CorporationId == user.CorporationId && x.TransferId == pagination.GuidId)
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

    public async Task<ActionResponse<TransferDetails>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Con el Producto y su categoria: el formulario de edicion los necesita para
            //preseleccionar los dos combos. Sin esto la pantalla de editar reventaba.
            var modelo = await _context.TransferDetails
                .AsNoTracking()
                .Include(x => x.Product)
                .ThenInclude(x => x!.ProductCategory)
                .FirstOrDefaultAsync(x => x.TransferDetailsId == id && x.CorporationId == corporationId);
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

    public async Task<ActionResponse<TransferDetails>> UpdateAsync(TransferDetails modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var existe = await _context.TransferDetails.AsNoTracking()
                .AnyAsync(x => x.TransferDetailsId == modelo.TransferDetailsId && x.CorporationId == corporationId);

            if (!existe)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<TransferDetails>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

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

    //Los seriales que se pueden elegir para una linea del traslado.
    //
    //Solo los que estan DISPONIBLES y en la bodega de ORIGEN de ese traslado, y que no
    //esten ya reservados por otra linea. Los que ya tiene esta linea se incluyen, para
    //poder editarla sin perder lo elegido.
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> GetAvailableSerialsAsync(
        Guid transferId, Guid productId, Guid? transferDetailsId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fallo<IEnumerable<GuidItemModel>>("Problemas de Validacion de Usuario");

            var traslado = await _context.Transfers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TransferId == transferId && x.CorporationId == user.CorporationId);

            if (traslado == null) return Fallo<IEnumerable<GuidItemModel>>("Problemas para Enconstrar el Registro Indicado");

            var lista = await _context.CargueDetails.AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.Cargue!.ProductId == productId &&
                            x.ProductStorageId == traslado.FromProductStorageId &&
                            x.Status == SerialStateType.Disponible &&
                            (x.TransferDetailsId == null ||
                             (transferDetailsId != null && x.TransferDetailsId == transferDetailsId)))
                .OrderBy(x => x.MacWlan)
                .Select(x => new GuidItemModel
                {
                    Value = x.CargueDetailId,
                    Name = x.MacWlan ?? string.Empty
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = lista };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //Los seriales que ya tiene reservados una linea
    //Los equipos que YA se movieron en esta linea. Sale del historico, no del serial:
    //el serial solo sabe en que bodega esta ahora, el historico sabe en cual traslado viajo.
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> GetMovedSerialsAsync(Guid transferDetailsId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<GuidItemModel>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var lista = await _context.TransferDetailSerials.AsNoTracking()
                .Where(x => x.TransferDetailsId == transferDetailsId &&
                            x.CorporationId == user.CorporationId)
                .OrderBy(x => x.MacWlan)
                .Select(x => new GuidItemModel
                {
                    Value = x.CargueDetailId,
                    Name = x.MacWlan
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<GuidItemModel>>
            {
                WasSuccess = true,
                Result = lista
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> GetLineSerialsAsync(Guid transferDetailsId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fallo<IEnumerable<GuidItemModel>>("Problemas de Validacion de Usuario");

            var lista = await _context.CargueDetails.AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.TransferDetailsId == transferDetailsId)
                .OrderBy(x => x.MacWlan)
                .Select(x => new GuidItemModel
                {
                    Value = x.CargueDetailId,
                    Name = x.MacWlan ?? string.Empty
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = lista };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //Guarda que seriales van en una linea. La CANTIDAD de la linea pasa a ser cuantos
    //seriales se eligieron: en un producto con serial el equipo es la unidad, no el numero.
    public async Task<ActionResponse<bool>> SaveSerialsAsync(Guid transferDetailsId, List<Guid> serialIds, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollback<bool>("Problemas de Validacion de Usuario");

            var linea = await _context.TransferDetails
                .Include(x => x.Transfer)
                .FirstOrDefaultAsync(x => x.TransferDetailsId == transferDetailsId &&
                                          x.CorporationId == user.CorporationId);

            if (linea == null) return await FalloRollback<bool>("Problemas para Enconstrar el Registro Indicado");

            if (linea.Transfer!.Status != TransferType.Pendiente)
            {
                return await FalloRollback<bool>("El traslado ya esta cerrado: no se pueden cambiar sus seriales.");
            }

            //Se sueltan los que tenia y se reservan los nuevos
            var anteriores = await _context.CargueDetails
                .Where(x => x.TransferDetailsId == transferDetailsId)
                .ToListAsync();

            foreach (var serial in anteriores)
            {
                serial.TransferDetailsId = null;
            }

            if (serialIds.Count > 0)
            {
                var nuevos = await _context.CargueDetails
                    .Where(x => serialIds.Contains(x.CargueDetailId) &&
                                x.CorporationId == user.CorporationId &&
                                x.ProductStorageId == linea.Transfer.FromProductStorageId &&
                                x.Status == SerialStateType.Disponible &&
                                (x.TransferDetailsId == null || x.TransferDetailsId == transferDetailsId))
                    .ToListAsync();

                //Si alguno ya no esta disponible o lo tomo otra linea, no se guarda nada
                if (nuevos.Count != serialIds.Count)
                {
                    return await FalloRollback<bool>("Alguno de los seriales ya no esta disponible en la bodega de origen.");
                }

                foreach (var serial in nuevos)
                {
                    serial.TransferDetailsId = transferDetailsId;
                }
            }

            linea.Quantity = serialIds.Count;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    private static ActionResponse<T> Fallo<T>(string mensaje) => new() { WasSuccess = false, Message = mensaje };

    private async Task<ActionResponse<T>> FalloRollback<T>(string mensaje)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fallo<T>(mensaje);
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

            //ANTES de tocar nada: un producto con serial no se puede cerrar sin sus equipos
            //elegidos. Si falta uno, no se mueve ni el numero ni los seriales.
            foreach (var item in transferdetails)
            {
                var llevaSerial = await _context.Products.AsNoTracking()
                    .Where(x => x.ProductId == item.ProductId)
                    .Select(x => x.WithSerials)
                    .FirstOrDefaultAsync();

                if (!llevaSerial)
                {
                    continue;
                }

                var elegidos = await _context.CargueDetails
                    .CountAsync(x => x.TransferDetailsId == item.TransferDetailsId);

                if (elegidos != item.Quantity)
                {
                    return new ActionResponse<Transfer>
                    {
                        WasSuccess = false,
                        Message = $"El producto {item.NameProduct} lleva serial: hay {elegidos} equipos elegidos y la cantidad dice {item.Quantity:N0}."
                    };
                }
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
                //Y los SERIALES que el operador eligio para esta linea se van con el
                //equipo: dejan de estar en la bodega de origen y pasan a la de destino.
                //Sin esto el numero se movia pero los equipos se quedaban donde estaban.
                var seriales = await _context.CargueDetails
                    .Where(x => x.TransferDetailsId == item.TransferDetailsId)
                    .ToListAsync();

                foreach (var serial in seriales)
                {
                    //Queda el historico ANTES de soltar la reserva: el serial solo sabe en
                    //que bodega esta AHORA, asi que sin esta fila no habria forma de saber
                    //despues que equipos viajaron en este traslado.
                    _context.TransferDetailSerials.Add(new TransferDetailSerial
                    {
                        TransferDetailsId = item.TransferDetailsId,
                        CargueDetailId = serial.CargueDetailId,
                        MacWlan = serial.MacWlan,
                        DateMoved = DateTime.Now,
                        CorporationId = item.CorporationId
                    });

                    serial.ProductStorageId = modelo.ToProductStorageId;
                    serial.TransferDetailsId = null;
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

            //Auditoria del cierre: quien lo cerro y cuando. El nombre queda congelado,
            //igual que el de quien lo creo.
            UpdateTrans.UserIdClosed = user.Id;
            UpdateTrans.NombreUsuarioCierre = $"{user.FirstName} {user.LastName}".Trim();
            UpdateTrans.DateClosed = DateTime.Now;

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

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var DataRemove = await _context.TransferDetails
                .FirstOrDefaultAsync(x => x.TransferDetailsId == id && x.CorporationId == corporationId);
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

    //El CorporationId sale del JWT: el controlador baja el username y aqui se resuelve.
    //Mismo patron que Compras y el resto de los modulos.
    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }
}
