using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceEntities;
using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xNotification.Interfaces;

namespace Spix.Services.ImplementEntties;

public class ManagerService : IManagerService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IFileStorage _fileStorage;
    private readonly IUserHelper _userHelper;
    private readonly IEmailHelper _emailHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IMapperService _mapperService;
    private readonly ImgSetting _imgOption;

    public ManagerService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IMemoryCache cache, IFileStorage fileStorage,
        IUserHelper userHelper, IEmailHelper emailHelper, IOptions<ImgSetting> ImgOption,
        HttpErrorHandler httpErrorHandler, IStringLocalizer localizer, IMapperService mapperService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _userHelper = userHelper;
        _emailHelper = emailHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _mapperService = mapperService;
        _imgOption = ImgOption.Value;
    }

    public async Task<ActionResponse<IEnumerable<Manager>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.Managers.Include(x => x.Corporation).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(u.LastName, $"%{filter}%") ||
                    EF.Functions.Like(u.FirstName + " " + u.LastName, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.FirstName).Paginate(pagination).ToListAsync();

            await Task.WhenAll(modelo.Select(async option =>
            {
                if (!string.IsNullOrWhiteSpace(option.Imagen))
                {
                    var FileResult = await _fileStorage.GetBlobSasUrlAsync(option.Imagen, _imgOption.ImgManager, TimeSpan.FromMinutes(3));
                    option.ImageFullPath = FileResult;
                }
            }));

            return new ActionResponse<IEnumerable<Manager>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Manager>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Manager>> GetAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ActionResponse<Manager>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_InvalidId"]
                };
            }
            var modelo = await _context.Managers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ManagerId == id);
            if (modelo == null)
            {
                return new ActionResponse<Manager>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }
            //Manejo de las imagenes desde Azure Private
            if (!string.IsNullOrWhiteSpace(modelo.Imagen))
            {
                var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.Imagen, _imgOption.ImgManager, TimeSpan.FromMinutes(2));
                modelo.ImageFullPath = FileResult;
            }
            else
            {
                modelo.ImageFullPath = _imgOption.ImgNoImage;
            }

            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Manager>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Manager>> UpdateAsync(Manager modelo, string frontUrl)
    {
        User CheckUserName = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
        if (CheckUserName != null)
        {
            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Message = _localizer["Generic_UserNameAlreadyUsed"]
            };
        }
        User CheckEmail = await _userHelper.GetUserByEmailAsync(modelo.Email);
        if (CheckEmail != null)
        {
            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Message = _localizer["Generic_EmailAlreadyUsed"]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            Manager NewModelo = new()
            {
                ManagerId = modelo.ManagerId,
                FirstName = modelo.FirstName,
                LastName = modelo.LastName,
                NroDocument = modelo.NroDocument,
                PhoneNumber = modelo.PhoneNumber,
                Address = modelo.Address,
                Email = modelo.Email,
                UserName = modelo.UserName,
                CorporationId = modelo.CorporationId,
                Job = modelo.Job,
                UserType = modelo.UserType,
                Imagen = modelo.Imagen,
                Active = modelo.Active,
            };
            if (modelo.ImgBase64 != null)
            {
                NewModelo.ImgBase64 = modelo.ImgBase64;
            }

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
                //NewModelo.Imagen = await _fileStorage.UploadImage(imageId, _imgOption.ImgManager!, guid);
                NewModelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgManager);
            }
            _context.Managers.Update(NewModelo);
            await _transactionManager.SaveChangesAsync();

            User UserCurrent = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
            if (UserCurrent != null)
            {
                UserCurrent.FirstName = modelo.FirstName;
                UserCurrent.LastName = modelo.LastName;
                UserCurrent.PhoneNumber = modelo.PhoneNumber;
                UserCurrent.Email = modelo.Email;
                UserCurrent.PhotoUser = modelo.Imagen;
                UserCurrent.JobPosition = modelo.Job;
                UserCurrent.Active = modelo.Active;
                IdentityResult result = await _userHelper.UpdateUserAsync(UserCurrent);
            }
            else
            {
                if (modelo.Active)
                {
                    Response response = await AcivateUser(modelo, frontUrl);
                    if (response.IsSuccess == false)
                    {
                        var guid = modelo.Imagen;
                        await _fileStorage.RemoveFileAsync(_imgOption.ImgManager!, guid!);
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<Manager>
                        {
                            WasSuccess = true,
                            Message = _localizer["Generic_UserCreationFail"]
                        };
                    }
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Manager>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Manager>> AddAsync(Manager Newmodelo, string frontUrl)
    {
        User CheckUserName = await _userHelper.GetUserByUserNameAsync(Newmodelo.UserName);
        if (CheckUserName != null)
        {
            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Message = _localizer["Generic_UserNameAlreadyUsed"]
            };
        }
        User CheckEmail = await _userHelper.GetUserByEmailAsync(Newmodelo.Email);
        if (CheckEmail != null)
        {
            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Message = _localizer["Generic_EmailAlreadyUsed"]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            Manager modelo = new()
            {
                ManagerId = Newmodelo.ManagerId,
                FirstName = Newmodelo.FirstName,
                LastName = Newmodelo.LastName,
                NroDocument = Newmodelo.NroDocument,
                PhoneNumber = Newmodelo.PhoneNumber,
                Address = Newmodelo.Address,
                Email = Newmodelo.Email,
                UserName = Newmodelo.UserName,
                CorporationId = Newmodelo.CorporationId,
                Job = Newmodelo.Job,
                UserType = UserType.Administrator,  //Tipo de UserRole
                Imagen = Newmodelo.Imagen,
                Active = Newmodelo.Active,
            };
            if (Newmodelo.ImgBase64 != null)
            {
                modelo.ImgBase64 = Newmodelo.ImgBase64;
            }
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                //modelo.Imagen = await _fileStorage.UploadImage(imageId, _imgOption.ImgManager!, guid);
                modelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgManager);
            }

            _context.Managers.Add(modelo);
            await _transactionManager.SaveChangesAsync();

            //Registro del Usuario en User
            if (modelo.Active)
            {
                Response response = await AcivateUser(modelo, frontUrl);
                if (!response.IsSuccess)
                {
                    var guid = modelo.Imagen;
                    await _fileStorage.RemoveFileAsync(_imgOption.ImgManager!, guid!);
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<Manager>
                    {
                        WasSuccess = true,
                        Message = _localizer["Generic_UserCreationFail"]
                    };
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Manager>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Manager>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Managers.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }
            var user = await _userHelper.GetUserByUserNameAsync(DataRemove.UserName);
            var RemoveRoleDetail = await _context.UserRoleDetails.Where(x => x.UserId == user.Id).ToListAsync();
            if (RemoveRoleDetail != null)
            {
                _context.UserRoleDetails.RemoveRange(RemoveRoleDetail!);
            }
            await _userHelper.DeleteUser(DataRemove.UserName);

            _context.Managers.Remove(DataRemove);

            if (DataRemove.Imagen is not null)
            {
                bool response = await _fileStorage.RemoveFileAsync(_imgOption.ImgManager!, DataRemove.Imagen);
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
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }

    private async Task<Response> AcivateUser(Manager manager, string frontUrl)
    {
        User user = await _userHelper.AddUserUsuarioAsync(manager.FirstName, manager.LastName, manager.UserName, manager.Email,
            manager.PhoneNumber, manager.Address, manager.Job, manager.CorporationId, manager.Imagen!, "Manager", manager.Active, manager.UserType);

        //Envio de Correo con Token de seguridad para Verificar el correo
        string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

        // Construir la URL sin `Url.Action`
        string tokenLink = $"{frontUrl}/api/accounts/ConfirmEmail?userid={user.Id}&token={myToken}";

        string subject = "Activacion de Cuenta";
        string body = ($"De: NexxtPlanet" +
            $"<h1>Email Confirmation</h1>" +
            $"<p>" +
            $"Su Clave Temporal es: <h2> \"{user.Pass}\"</h2>" +
            $"</p>" +
            $"Para Activar su vuenta, " +
            $"Has Click en el siguiente Link:</br></br><strong><a href = \"{tokenLink}\">Confirmar Correo</a></strong>");

        Response response = await _emailHelper.ConfirmarCuenta(user.Email!, $"{user.FirstName} {user.LastName}", subject, body);
        if (response.IsSuccess == false)
        {
            return response;
        }
        return response;
    }
}