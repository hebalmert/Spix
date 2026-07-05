using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;

namespace Spix.AppService.ImplementSchedule;

public class ServiceRequestPicService : IServiceRequestPicService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly ImgSetting _imgOption;
    private readonly IFileStorage _fileStorage;

    public ServiceRequestPicService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, IMapperService mapperService, HttpErrorHandler httpErrorHandler,
        IOptions<ImgSetting> imgOption, IFileStorage fileStorage)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _httpErrorHandler = httpErrorHandler;
        _imgOption = imgOption.Value;
        _fileStorage = fileStorage;
    }

    public async Task<ActionResponse<ServiceRequestPic>> GetByServiceRequestAsync(Guid serviceRequestId, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
                return AuthFail<ServiceRequestPic>();

            var modelo = await _context.ServiceRequestPics
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == serviceRequestId && x.CorporationId == user.CorporationId);

            if (modelo == null)
            {
                var requestExists = await _context.ServiceRequests.AnyAsync(x =>
                    x.ServiceRequestId == serviceRequestId &&
                    x.CorporationId == user.CorporationId &&
                    x.Active);
                if (!requestExists)
                    return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "Solicitud de servicio no encontrada." };

                return new ActionResponse<ServiceRequestPic>
                {
                    WasSuccess = true,
                    Result = new ServiceRequestPic { ServiceRequestId = serviceRequestId }
                };
            }

            await LoadImagesAsync(modelo);
            return new ActionResponse<ServiceRequestPic> { WasSuccess = true, Result = modelo };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestPic>(ex);
        }
    }

    public async Task<ActionResponse<ServiceRequestPic>> GetAsync(Guid id, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
                return AuthFail<ServiceRequestPic>();

            var modelo = await _context.ServiceRequestPics
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestPicId == id && x.CorporationId == user.CorporationId);

            if (modelo == null)
                return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "Problemas para Enconstrar el Registro Indicado" };

            await LoadImagesAsync(modelo);
            return new ActionResponse<ServiceRequestPic> { WasSuccess = true, Result = modelo };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestPic>(ex);
        }
    }

    public async Task<ActionResponse<ServiceRequestPic>> UpdateAsync(ServiceRequestPic modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestPic>();
            }

            var current = await _context.ServiceRequestPics
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestPicId == modelo.ServiceRequestPicId && x.CorporationId == user.CorporationId);
            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "Problemas para Enconstrar el Registro Indicado" };
            }

            if (current.ServiceRequest!.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "La solicitud completada no permite modificar fotos." };
            }

            current.ImgBefore1Base64 = modelo.ImgBefore1Base64;
            current.ImgBefore2Base64 = modelo.ImgBefore2Base64;
            current.ImgAfter1Base64 = modelo.ImgAfter1Base64;
            current.ImgAfter2Base64 = modelo.ImgAfter2Base64;

            await SaveImagesAsync(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return await GetAsync(current.ServiceRequestPicId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestPic>(ex);
        }
    }

    public async Task<ActionResponse<ServiceRequestPic>> AddAsync(ServiceRequestPic modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestPic>();
            }

            var request = await _context.ServiceRequests.FirstOrDefaultAsync(x =>
                x.ServiceRequestId == modelo.ServiceRequestId &&
                x.CorporationId == user.CorporationId &&
                x.Active);
            if (request == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "Solicitud de servicio no encontrada." };
            }

            if (request.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "La solicitud completada no permite agregar fotos." };
            }

            var exists = await _context.ServiceRequestPics.AnyAsync(x => x.ServiceRequestId == modelo.ServiceRequestId);
            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ServiceRequestPic> { WasSuccess = false, Message = "La solicitud ya tiene fotos registradas." };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            modelo.DateCreated = DateTime.Now;
            modelo.UsuarioOwner = $"{user.FirstName!} {user.LastName!}";
            modelo.UserId = Guid.Parse(user.Id);

            await SaveImagesAsync(modelo);

            _context.ServiceRequestPics.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return await GetAsync(modelo.ServiceRequestPicId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestPic>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<bool>();
            }

            var modelo = await _context.ServiceRequestPics
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestPicId == id && x.CorporationId == user.CorporationId);
            if (modelo == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "Problemas para Enconstrar el Registro Indicado" };
            }

            if (modelo.ServiceRequest!.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "La solicitud completada no permite eliminar fotos." };
            }

            DeleteImage(modelo.PhotoBefore1);
            DeleteImage(modelo.PhotoBefore2);
            DeleteImage(modelo.PhotoAfter1);
            DeleteImage(modelo.PhotoAfter2);

            _context.ServiceRequestPics.Remove(modelo);
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

    private async Task SaveImagesAsync(ServiceRequestPic modelo)
    {
        modelo.PhotoBefore1 = await SaveImageAsync(modelo.ImgBefore1Base64, modelo.PhotoBefore1);
        modelo.PhotoBefore2 = await SaveImageAsync(modelo.ImgBefore2Base64, modelo.PhotoBefore2);
        modelo.PhotoAfter1 = await SaveImageAsync(modelo.ImgAfter1Base64, modelo.PhotoAfter1);
        modelo.PhotoAfter2 = await SaveImageAsync(modelo.ImgAfter2Base64, modelo.PhotoAfter2);
    }

    private async Task<string?> SaveImageAsync(string? imageBase64, string? currentName)
    {
        if (string.IsNullOrWhiteSpace(imageBase64))
            return currentName;

        var fileName = currentName ?? $"{Guid.NewGuid()}.jpg";
        var imageId = Convert.FromBase64String(imageBase64);
        return await _fileStorage.SaveImageAsync(imageId, fileName, _imgOption.ImgContractIDPic);
    }

    private async Task LoadImagesAsync(ServiceRequestPic modelo)
    {
        modelo.ImageBefore1FullPath = await GetImageAsync(modelo.PhotoBefore1);
        modelo.ImageBefore2FullPath = await GetImageAsync(modelo.PhotoBefore2);
        modelo.ImageAfter1FullPath = await GetImageAsync(modelo.PhotoAfter1);
        modelo.ImageAfter2FullPath = await GetImageAsync(modelo.PhotoAfter2);
    }

    private async Task<string?> GetImageAsync(string? photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            return _imgOption.ImgNoImage;

        return await _fileStorage.GetBlobSasUrlAsync(photo, _imgOption.ImgContractIDPic, TimeSpan.FromMinutes(2));
    }

    private void DeleteImage(string? photo)
    {
        if (!string.IsNullOrWhiteSpace(photo))
            _fileStorage.DeleteImage(_imgOption.ImgContractIDPic!, photo);
    }

    private async Task<User?> GetUserAsync(string username) => await _userHelper.GetUserByUserNameAsync(username);

    private static ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = "Problemas de Validacion de Usuario"
    };
}
