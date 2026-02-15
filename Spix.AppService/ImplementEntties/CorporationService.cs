using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceEntities;
using Spix.Domain.Entities;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;

namespace Spix.Services.ImplementEntties;

public class CorporationService : ICorporationService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IFileStorage _fileStorage;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly ImgSetting _imgOption;

    public CorporationService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IMemoryCache cache, IFileStorage fileStorage,
        IOptions<ImgSetting> ImgOption, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _imgOption = ImgOption.Value;
    }

    public async Task<ActionResponse<IEnumerable<Corporation>>> ComboAsync()
    {
        try
        {
            var ListModel = await _context.Corporations.Where(x => x.Active).OrderBy(x => x.Name).ToListAsync();
            // Insertar el elemento neutro al inicio
            var defaultItem = new Corporation
            {
                CorporationId = 0,
                Name = "[Select Corporation]",
                Active = true
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<Corporation>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Corporation>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Corporation>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.Corporations.Include(x => x.SoftPlan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                //Busqueda grandes mateniendo los indices de los campos, campo Esta Collation CI para Case Insensitive
                queryable = queryable.Where(u => EF.Functions.Like(u.Name, $"%{pagination.Filter}%"));
            }
            var result = await queryable.ApplyFullPaginationAsync(_httpContextAccessor.HttpContext!, pagination);

            await Task.WhenAll(result.Select(async corp =>
            {
                if (string.IsNullOrWhiteSpace(corp.Imagen))
                {
                    corp.ImageFullPath = _imgOption.ImgNoImage; // imagen pública libre
                }
                else
                {
                    var FileResult = await _fileStorage.GetBlobSasUrlAsync(corp.Imagen, _imgOption.ImgCorporation, TimeSpan.FromMinutes(2));
                    corp.ImageFullPath = FileResult;
                }
            }));

            return new ActionResponse<IEnumerable<Corporation>>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Corporation>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Corporation>> GetAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ActionResponse<Corporation>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_InvalidId"]
                };
            }
            var modelo = await _context.Corporations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CorporationId == id);
            if (modelo == null)
            {
                return new ActionResponse<Corporation>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }

            if (string.IsNullOrWhiteSpace(modelo.Imagen))
            {
                modelo.ImageFullPath = _imgOption.ImgNoImage; // imagen pública libre
            }
            else
            {
                var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.Imagen, _imgOption.ImgCorporation, TimeSpan.FromMinutes(2));
                modelo.ImageFullPath = FileResult;
            }
            return new ActionResponse<Corporation>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Corporation>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Corporation>> UpdateAsync(Corporation modelo)
    {
        if (modelo == null || modelo.CorporationId <= 0)
        {
            return new ActionResponse<Corporation>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidId"]
            };
        }
        await _transactionManager.BeginTransactionAsync();
        try
        {
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid;
                if (modelo.Imagen == null)
                {
                    guid = Guid.NewGuid().ToString() + ".jpg";
                }
                else
                {
                    guid = modelo.Imagen;
                }
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                //modelo.Imagen = await _fileStorage.UploadImage(imageId, _imgOption.ImgCorporation!, guid);
                modelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgCorporation);
            }

            _context.Corporations.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Corporation>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer["Generic_Success"]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Corporation>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Corporation>> AddAsync(Corporation modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<Corporation>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidModel"] // 🧠 Clave multilenguaje para modelo nulo
            };
        }
        await _transactionManager.BeginTransactionAsync();
        try
        {
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                //modelo.Imagen = await _fileStorage.UploadImage(imageId, _imgOption.ImgCorporation!, guid);
                modelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgCorporation);
            }

            _context.Corporations.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Corporation>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer["Generic_Success"]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Corporation>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Corporations.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"] // 🌐 Localizado para no encontrado
                };
            }

            _context.Corporations.Remove(DataRemove);

            if (DataRemove.Imagen is not null)
            {
                bool response = await _fileStorage.RemoveFileAsync(_imgOption.ImgCorporation!, DataRemove.Imagen);
                if (!response)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = _localizer["Generic_RecordDeletedNoImage"]
                    };
                }
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = _localizer["Generic_Success"]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}