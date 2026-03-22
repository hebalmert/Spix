using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;

namespace Spix.Services.ImplementInven;

public class SupplierService : ISupplierService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IUserHelper _userHelper;
    private readonly ImgSetting _imgOption;

    public SupplierService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, IFileStorage fileStorage, HttpErrorHandler httpErrorHandle,
        IUserHelper userHelper, IOptions<ImgSetting> ImgOption)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _userHelper = userHelper;
        _imgOption = ImgOption.Value;
        _httpErrorHandler = httpErrorHandle;
    }

    public async Task<ActionResponse<IEnumerable<Supplier>>> ComboAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Supplier>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }
            var ListModel = await _context.Suppliers.Where(x => x.Active && x.CorporationId == user.CorporationId).ToListAsync();

            return new ActionResponse<IEnumerable<Supplier>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Supplier>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Supplier>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Supplier>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.Suppliers.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.Name).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Supplier>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Supplier>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Supplier>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.Suppliers
                .Include(x => x.State).ThenInclude(x => x.Cities)
                .Include(x => x.DocumentType)
                .FirstOrDefaultAsync(x => x.SupplierId == id);
            if (modelo == null)
            {
                return new ActionResponse<Supplier>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<Supplier>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Supplier>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Supplier>> UpdateAsync(Supplier modelo, string frontUrl)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            Supplier NewModelo = _mapperService.Map<Supplier, Supplier>(modelo);
            if (modelo.ImgBase64 != null)
            {
                NewModelo.ImgBase64 = modelo.ImgBase64;
            }

            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid;
                if (modelo.Photo == null)
                {
                    guid = Guid.NewGuid().ToString() + ".jpg";
                }
                else
                {
                    guid = modelo.Photo;
                }
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                NewModelo.Photo = await _fileStorage.SaveImageAsync(imageId, _imgOption.ImgSuppliers!, guid);
            }
            _context.Suppliers.Update(NewModelo);
            await _transactionManager.SaveChangesAsync();

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Supplier>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Supplier>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Supplier>> AddAsync(Supplier modelo, string username, string frontUrl)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Supplier>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                modelo.Photo = await _fileStorage.SaveImageAsync(imageId, _imgOption.ImgSuppliers!, guid);
            }

            _context.Suppliers.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Supplier>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Supplier>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Suppliers.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            _context.Suppliers.Remove(DataRemove);

            if (DataRemove.Photo is not null)
            {
                var response = await _fileStorage.RemoveFileAsync(_imgOption.ImgSuppliers!, DataRemove.Photo);
                if (!response)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Se Elimino el Registro pero Sin la Imagen"
                    };
                }
            }

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