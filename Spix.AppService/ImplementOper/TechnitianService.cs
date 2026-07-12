using DocumentFormat.OpenXml.ExtendedProperties;
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
using Spix.AppInfra.SecretProtection;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.UtilityTools;
using Spix.AppService.InterfacesOper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesEmails;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EntitiesEmailDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xLanguage.Resources;
using Spix.xNotification.Interfaces;

namespace Spix.Services.ImplementOper;

public class TechnitianService : ITechnitianService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IUserHelper _userHelper;
    private readonly IEmailDeliveryService _emailDeliveryService;
    private readonly ISecretProtector _secretProtector;
    private readonly IUtilityTools _utilityTools;
    private readonly IStringLocalizer _localizer;
    private readonly ImgSetting _imgOption;

    public TechnitianService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, IFileStorage fileStorage, HttpErrorHandler httpErrorHandler,
        IUserHelper userHelper, IEmailDeliveryService emailDeliveryService, ISecretProtector secretProtector,
        IUtilityTools utilityTools, IOptions<ImgSetting> ImgOption, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _userHelper = userHelper;
        _emailDeliveryService = emailDeliveryService;
        _secretProtector = secretProtector;
        _utilityTools = utilityTools;
        _localizer = localizer;
        _imgOption = ImgOption.Value;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username)
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
            var ListModel = await _context.Technicians
                .Where(x => x.Active && x.CorporationId == user.CorporationId)
                .Select(x => new GuidItemModel { Value = x.TechnicianId, Name = x.FirstName + " " + x.LastName })
                .ToListAsync();
            ListModel.Insert(0, new GuidItemModel 
            { 
                Value = Guid.Empty, 
                Name = _localizer[nameof(Resource.Select_Technician)]
            });


            return new ActionResponse<IEnumerable<GuidItemModel>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Technician>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Technician>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.Technicians.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

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
                    var FileResult = await _fileStorage.GetBlobSasUrlAsync(option.Imagen, _imgOption.ImgTechnicians, TimeSpan.FromMinutes(3));
                    option.ImageFullPath = FileResult;
                }
                else
                {
                    option.ImageFullPath = _imgOption.ImgNoImage;
                }
            }));

            return new ActionResponse<IEnumerable<Technician>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Technician>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Technician>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.Technicians
                .AsNoTracking()
                .Include(x => x.DocumentType)
                .FirstOrDefaultAsync(x => x.TechnicianId == id);
            if (modelo == null)
            {
                return new ActionResponse<Technician>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            if (!string.IsNullOrWhiteSpace(modelo.Imagen))
            {
                var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.Imagen, _imgOption.ImgTechnicians, TimeSpan.FromMinutes(2));
                modelo.ImageFullPath = FileResult;
            }
            else
            {
                modelo.ImageFullPath = _imgOption.ImgNoImage;
            }
            return new ActionResponse<Technician>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Technician>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Technician>> UpdateAsync(Technician modelo, string frontUrl)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            Technician NewModelo = _mapperService.Map<Technician, Technician>(modelo);

            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid = modelo.Imagen == null ? Guid.NewGuid().ToString() + ".jpg" : modelo.Imagen;
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                NewModelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgTechnicians);
            }

            _context.Technicians.Update(NewModelo);
            await _transactionManager.SaveChangesAsync();

            User UserCurrent = await _userHelper.GetUserByUserNameAsync(modelo.UserName);

            if (UserCurrent != null)
            {
                // Actualizar solo si tiene cuenta
                UserCurrent.FirstName = modelo.FirstName;
                UserCurrent.LastName = modelo.LastName;
                UserCurrent.PhoneNumber = modelo.PhoneNumber;
                UserCurrent.PhotoUser = modelo.Imagen;
                UserCurrent.JobPosition = "Technician";
                UserCurrent.Active = modelo.Active;

                await _userHelper.UpdateUserAsync(UserCurrent);
            }
            else
            {
                if (modelo.Active)
                {
                    Response response = await AcivateUser(modelo, frontUrl);
                    if (!response.IsSuccess)
                    {
                        var guid = modelo.Imagen;
                        _fileStorage.DeleteImage(_imgOption.ImgTechnicians!, guid!);
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<Technician>
                        {
                            WasSuccess = false,
                            Message = _localizer["Generic_UserCreationFail"]
                        };
                    }
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Technician>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Technician>(ex);
        }
    }

    public async Task<ActionResponse<Technician>> AddAsync(Technician modelo, string username, string frontUrl)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            User UserLogin = await _userHelper.GetUserByUserNameAsync(username);
            if (UserLogin == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Technician>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthUserNameFail)]
                };
            }

            User CheckUserName = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
            if (CheckUserName != null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Technician>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_UserNameAlreadyUsed)]
                };
            }

            var user = await _userHelper.GetUserByEmailAsync(modelo.Email);
            if (user != null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Technician>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_EmailAlreadyUsed)]
                };
            }

            modelo.DateCreated = DateTime.Now;
            modelo.UserType = UserType.Technician;
            modelo.CorporationId = UserLogin.CorporationId!.Value;

            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                modelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgTechnicians);
            }

            _context.Technicians.Add(modelo);
            await _transactionManager.SaveChangesAsync();

            if (modelo.Active)
            {
                Response response = await AcivateUser(modelo, frontUrl);
                if (!response.IsSuccess)
                {
                    var guid = modelo.Imagen;
                    _fileStorage.DeleteImage(_imgOption.ImgTechnicians!, guid!);
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<Technician>
                    {
                        WasSuccess = false,
                        Message = "No se ha podido crear el Usuario, Intentelo de nuevo"
                    };
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Technician>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Technician>(ex);
        }
    }

    public async Task<ActionResponse<bool>> ResendActivationEmailAsync(Guid id, string frontUrl)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_InvalidId"]
                };
            }

            var modelo = await _context.Technicians.AsNoTracking().FirstOrDefaultAsync(x => x.TechnicianId == id);
            if (modelo == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            if (!modelo.Active)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El Tecnico no esta activo."
                };
            }

            User user = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
            if (user == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No existe un usuario creado para este Tecnico."
                };
            }

            if (user.EmailConfirmed)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "La cuenta de este Tecnico ya fue confirmada."
                };
            }

            var password = _utilityTools.GeneratePass(8);
            var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userHelper.ResetPasswordAsync(user, resetToken, password);
            if (!resetResult.Succeeded)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se pudo generar una nueva clave temporal para el usuario."
                };
            }

            user.Pass = password;

            Response response = await SendActivationEmailAsync(user, frontUrl);
            if (!response.IsSuccess)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = response.Message ?? "No se pudo enviar el correo de activacion."
                };
            }

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Technicians.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            var user = await _userHelper.GetUserByUserNameAsync(DataRemove.UserName);
            if (user != null)
            {
                var RemoveRoleDetail = await _context.UserRoleDetails.Where(x => x.UserId == user.Id).ToListAsync();
                if (RemoveRoleDetail != null)
                {
                    _context.UserRoleDetails.RemoveRange(RemoveRoleDetail!);
                }
                await _userHelper.DeleteUser(DataRemove.UserName);
            }

            _context.Technicians.Remove(DataRemove);

            if (DataRemove.Imagen is not null)
            {
                var response = await _fileStorage.RemoveFileAsync(_imgOption.ImgTechnicians!, DataRemove.Imagen);
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

    private async Task<Response> AcivateUser(Technician modelo, string frontUrl)
    {
        User user = await _userHelper.AddUserUsuarioAsync(modelo.FirstName, modelo.LastName, modelo.UserName, modelo.Email,
            modelo.PhoneNumber, modelo.Address, "Technician", modelo.CorporationId, modelo.Imagen!, "Technician", modelo.Active, modelo.UserType);

        if (user == null)
        {
            return new Response
            {
                IsSuccess = false,
                Message = _localizer["Generic_UserCreationFail"]
            };
        }

        return await SendActivationEmailAsync(user, frontUrl);
    }

    private async Task<Response> SendActivationEmailAsync(User user, string frontUrl)
    {
        //Envio de Correo con Token de seguridad para Verificar el correo
        string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

        // Construir la URL sin `Url.Action`
        string tokenLink = $"{frontUrl}/api/accounts/ConfirmEmail?userid={user.Id}&token={myToken}";

        string subject = _localizer["AccountActivation_Subject"];
        string body = Spix.AppService.ImplementEmails.LocalizedEmailTemplateFactory.BuildAccountActivation(_localizer, user.FirstName, user.LastName, user.Pass, tokenLink);

        Response response = await SendCorporateEmailAsync(user, subject, body);
        if (response.IsSuccess == false)
        {
            return response;
        }
        return response;
    }

    private async Task<Response> SendCorporateEmailAsync(User user, string subject, string body)
    {
        if (!user.CorporationId.HasValue)
        {
            return new Response
            {
                IsSuccess = false,
                Message = "El usuario no tiene corporacion asignada para enviar el correo."
            };
        }

        EmailProviderSetting? provider = await _context.EmailProviderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CorporationId == user.CorporationId.Value &&
                                      x.Active &&
                                      x.IsDefault);

        if (provider == null)
        {
            return new Response
            {
                IsSuccess = false,
                Message = "La corporacion no tiene proveedor de correo default activo."
            };
        }

        var model = new EmailDeliveryDTO
        {
            ProviderType = provider.ProviderType,
            SendGridApiKey = _secretProtector.Unprotect(provider.SendGridApiKeyEncrypted),
            SmtpHost = provider.SmtpHost,
            SmtpPort = provider.SmtpPort ?? 0,
            SmtpUseSsl = provider.SmtpUseSsl,
            SmtpUser = provider.SmtpUser,
            SmtpPassword = _secretProtector.Unprotect(provider.SmtpPasswordEncrypted),
            FromEmail = provider.FromEmail,
            FromName = provider.FromName,
            To = user.Email!,
            NameTo = $"{user.FirstName} {user.LastName}",
            Subject = subject,
            Body = body
        };

        return await _emailDeliveryService.SendAsync(model);
    }
}
