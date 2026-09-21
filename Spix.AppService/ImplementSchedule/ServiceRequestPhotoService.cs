using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementSchedule;

//Las fotos de la visita. Aparte porque toca blobs: subir, firmar la url y borrar el
//archivo son otra responsabilidad, con su propio contenedor y su propio tope.
public class ServiceRequestPhotoService : IServiceRequestPhotoService
{
    //Tope de fotos por orden
    private const int MaxPhotos = 4;

    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IFileStorage _fileStorage;
    private readonly ImgSetting _imgOption;

    public ServiceRequestPhotoService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IFileStorage fileStorage,
        IOptions<ImgSetting> imgOption)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _fileStorage = fileStorage;
        _imgOption = imgOption.Value;
    }
    public async Task<ActionResponse<ServiceRequestPhotoDto>> AddPhotoAsync(ServiceRequestPhotoDto dto, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestPhotoDto>();
            }

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.Active);

            if (request == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestPhotoDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //Una orden cerrada es evidencia: ya no se le agregan fotos
            //Mientras la visita no arranca no hay nada que cargarle: el tecnico
            //tiene que marcar que esta en sitio.
            if (request.ScheduleStatus == ScheduleStatus.Pending ||
                request.ScheduleStatus == ScheduleStatus.Requested)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestPhotoDto>(_localizer["Visit_NotStarted"]);
            }

            if (request.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestPhotoDto>(_localizer["Photo_ClosedOrder"]);
            }

            var total = await _context.ServiceRequestPhotos
                .CountAsync(x => x.ServiceRequestId == dto.ServiceRequestId);

            if (total >= MaxPhotos)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestPhotoDto>(_localizer["Photo_MaxReached"]);
            }

            if (string.IsNullOrWhiteSpace(dto.ImgBase64))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestPhotoDto>(_localizer["Photo_Empty"]);
            }

            var photo = new ServiceRequestPhoto
            {
                ServiceRequestPhotoId = Guid.NewGuid(),
                ServiceRequestId = request.ServiceRequestId,
                PhotoType = dto.PhotoType,
                DateCreated = DateTime.UtcNow,
                UserByName = $"{user.FirstName} {user.LastName}".Trim(),
                UserId = Guid.TryParse(user.Id, out var userId) ? userId : null,
                CorporationId = request.CorporationId
            };

            var bytes = Convert.FromBase64String(dto.ImgBase64);
            photo.Photo = await _fileStorage.SaveImageAsync(bytes, $"{photo.ServiceRequestPhotoId}.jpg", _imgOption.RequiereServicePicture);

            _context.ServiceRequestPhotos.Add(photo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            dto.ServiceRequestPhotoId = photo.ServiceRequestPhotoId;
            dto.DateCreated = photo.DateCreated;
            dto.UserByName = photo.UserByName;
            dto.ImgBase64 = null;
            dto.ImageFullPath = await GetPhotoUrlAsync(photo.Photo);

            return new ActionResponse<ServiceRequestPhotoDto> { WasSuccess = true, Result = dto };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestPhotoDto>(ex);
        }
    }


    public async Task<ActionResponse<bool>> DeletePhotoAsync(Guid id, string username)
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

            var photo = await _context.ServiceRequestPhotos
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestPhotoId == id &&
                                          x.CorporationId == user.CorporationId);

            if (photo == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (photo.ServiceRequest!.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["Photo_ClosedOrder"]);
            }

            //Primero la base, y si eso salio bien se suelta el archivo
            _context.ServiceRequestPhotos.Remove(photo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            if (!string.IsNullOrWhiteSpace(photo.Photo))
            {
                _fileStorage.DeleteImage(_imgOption.RequiereServicePicture!, photo.Photo);
            }

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //El enlace del blob dura poco: se pide cada vez que se abre la orden
    private async Task<string?> GetPhotoUrlAsync(string? photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            return _imgOption.ImgNoImage;

        return await _fileStorage.GetBlobSasUrlAsync(photo, _imgOption.RequiereServicePicture, TimeSpan.FromMinutes(5));
    }

    //Quien entra: si es tecnico, solo puede tocar lo suyo
    private async Task<User?> GetUserAsync(string username) => await _userHelper.GetUserByUserNameAsync(username);

    private async Task<Guid?> GetLoggedTechnicianIdAsync(User user)
    {
        var isTechnician = await _context.UserRoleDetails
            .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Technician);

        if (!isTechnician)
            return null;

        var technicianId = await _context.Technicians
            .Where(x => x.UserName == user.UserName &&
                        x.CorporationId == user.CorporationId &&
                        x.Active)
            .Select(x => (Guid?)x.TechnicianId)
            .FirstOrDefaultAsync();

        return technicianId ?? Guid.Empty;
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };

    private static ActionResponse<T> Fail<T>(string message) => new()
    {
        WasSuccess = false,
        Message = message
    };
}
