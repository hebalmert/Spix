using Microsoft.AspNetCore.Http;
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

namespace Spix.Services.ImplementOper
{
    public class ClientService : IClientService
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

        public ClientService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
            ITransactionManager transactionManager, IMemoryCache cache, IFileStorage fileStorage, HttpErrorHandler httpErrorHandle,
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
            _httpErrorHandler = httpErrorHandle;
        }

        public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username, string? filter)
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
                var query = _context.Clients
                    .Where(x => x.Active && x.CorporationId == user.CorporationId);

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = filter.Trim().ToLower();

                    query = query.Where(x =>
                        x.FirstName.ToLower().Contains(filter) ||
                        x.LastName.ToLower().Contains(filter) ||
                        x.Document.ToLower().Contains(filter) ||
                        x.Email.ToLower().Contains(filter)
                    );
                }

                var list = await query.OrderBy(x => x.FirstName) .ThenBy(x => x.LastName).Take(20)
                            .Select(x => new GuidItemModel
                            {
                                Value = x.ClientId,
                                Name = x.FirstName + " " + x.LastName
                            })
                            .ToListAsync();

                return new ActionResponse<IEnumerable<GuidItemModel>>
                {
                    WasSuccess = true,
                    Result = list
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex); // ✅ Manejo de errores automático
            }
        }

        public async Task<ActionResponse<IEnumerable<Client>>> GetAsync(PaginationDTO pagination, string username)
        {
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<IEnumerable<Client>>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                var queryable = _context.Clients.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

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
                        var FileResult = await _fileStorage.GetBlobSasUrlAsync(option.Imagen, _imgOption.ImgClient, TimeSpan.FromMinutes(3));
                        option.ImageFullPath = FileResult;
                    }
                    else
                    {
                        option.ImageFullPath = _imgOption.ImgNoImage;
                    }
                }));

                return new ActionResponse<IEnumerable<Client>>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Client>>(ex); // ✅ Manejo de errores automático
            }
        }

        public async Task<ActionResponse<Client>> GetAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new ActionResponse<Client>
                    {
                        WasSuccess = false,
                        Message = _localizer["Generic_InvalidId"]
                    };
                }
                var modelo = await _context.Clients
                    .AsNoTracking()
                    .Include(x => x.DocumentType)
                    .FirstOrDefaultAsync(x => x.ClientId == id);
                if (modelo == null)
                {
                    return new ActionResponse<Client>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }

                if (!string.IsNullOrWhiteSpace(modelo.Imagen))
                {
                    var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.Imagen, _imgOption.ImgClient, TimeSpan.FromMinutes(2));
                    modelo.ImageFullPath = FileResult;
                }
                else
                {
                    modelo.ImageFullPath = _imgOption.ImgNoImage;
                }

                return new ActionResponse<Client>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<Client>(ex); // ✅ Manejo de errores automático
            }
        }

        public async Task<ActionResponse<Client>> UpdateAsync(Client modelo, string frontUrl)
        {
            await _transactionManager.BeginTransactionAsync();

            try
            {
                Client NewModelo = _mapperService.Map<Client, Client>(modelo);

                if (!string.IsNullOrEmpty(modelo.ImgBase64))
                {
                    string guid = modelo.Imagen == null ? Guid.NewGuid().ToString() + ".jpg" : modelo.Imagen;
                    var imageId = Convert.FromBase64String(modelo.ImgBase64);
                    NewModelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgClient);
                }

                _context.Clients.Update(NewModelo);
                await _transactionManager.SaveChangesAsync();

                User UserCurrent = await _userHelper.GetUserByUserNameAsync(modelo.UserName);

                if (UserCurrent != null)
                {
                    if (!modelo.CreateAccount)
                    {
                        await _userHelper.DeleteUser(modelo.UserName);
                    }
                    else
                    {
                        // Actualizar solo si tiene cuenta
                        UserCurrent.FirstName = modelo.FirstName;
                        UserCurrent.LastName = modelo.LastName;
                        UserCurrent.PhoneNumber = modelo.PhoneNumber;
                        UserCurrent.PhotoUser = modelo.Imagen;
                        UserCurrent.JobPosition = "Client";

                        // Active del IdentityUser ahora depende de CreateAccount
                        UserCurrent.Active = modelo.CreateAccount;

                        await _userHelper.UpdateUserAsync(UserCurrent);
                    }
                }
                else
                {
                    // Crear usuario solo si CreateAccount = true
                    if (modelo.CreateAccount)
                    {
                        Response response = await AcivateUser(modelo, frontUrl);
                        if (!response.IsSuccess)
                        {
                            var guid = modelo.Imagen;
                            _fileStorage.DeleteImage(_imgOption.ImgClient!, guid!);
                            await _transactionManager.RollbackTransactionAsync();
                            return new ActionResponse<Client>
                            {
                                WasSuccess = false,
                                Message = _localizer["Generic_UserCreationFail"]
                            };
                        }
                    }
                }

                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<Client>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<Client>(ex);
            }
        }

        public async Task<ActionResponse<Client>> AddAsync(Client modelo, string username, string frontUrl)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                User UserLogin = await _userHelper.GetUserByUserNameAsync(username);
                if (UserLogin == null)
                {
                    return new ActionResponse<Client>
                    {
                        WasSuccess = false,
                        Message = _localizer[nameof(Resource.Generic_AuthUserNameFail)]
                    };
                }

                User CheckUserName = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
                if (CheckUserName != null)
                {
                    return new ActionResponse<Client>
                    {
                        WasSuccess = false,
                        Message = _localizer[nameof(Resource.Generic_UserNameAlreadyUsed)]
                    };
                }

                var user = await _userHelper.GetUserByEmailAsync(modelo.Email);
                if (user != null)
                {
                    return new ActionResponse<Client>
                    {
                        WasSuccess = false,
                        Message = _localizer[nameof(Resource.Generic_EmailAlreadyUsed)]
                    };
                }

                modelo.DateCreated = DateTime.Now;
                modelo.UserType = UserType.Client;
                modelo.CorporationId = UserLogin.CorporationId!.Value;

                if (!string.IsNullOrEmpty(modelo.ImgBase64))
                {
                    string guid = Guid.NewGuid().ToString() + ".jpg";
                    var imageId = Convert.FromBase64String(modelo.ImgBase64);
                    modelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgClient);
                }

                _context.Clients.Add(modelo);
                await _transactionManager.SaveChangesAsync();

                // Crear cuenta SOLO si CreateAccount = true
                if (modelo.CreateAccount)
                {
                    Response response = await AcivateUser(modelo, frontUrl);
                    if (!response.IsSuccess)
                    {
                        var guid = modelo.Imagen;
                        _fileStorage.DeleteImage(_imgOption.ImgClient!, guid!);
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<Client>
                        {
                            WasSuccess = false,
                            Message = "No se ha podido crear el Usuario, Intentelo de nuevo"
                        };
                    }
                }

                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<Client>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<Client>(ex);
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

                var modelo = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == id);
                if (modelo == null)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }

                if (!modelo.Active || !modelo.CreateAccount)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "El Cliente no tiene activa la creacion de cuenta."
                    };
                }

                User user = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
                if (user == null)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "No existe un usuario creado para este Cliente."
                    };
                }

                if (user.EmailConfirmed)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "La cuenta de este Cliente ya fue confirmada."
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
                var DataRemove = await _context.Clients.FindAsync(id);
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

                _context.Clients.Remove(DataRemove);

                if (DataRemove.Imagen is not null)
                {
                    var response = _fileStorage.DeleteImage(_imgOption.ImgClient!, DataRemove.Imagen);
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

        private async Task<Response> AcivateUser(Client modelo, string frontUrl)
        {
            User user = await _userHelper.AddUserUsuarioAsync(modelo.FirstName, modelo.LastName, modelo.UserName, modelo.Email,
                modelo.PhoneNumber, modelo.Address, "Client", modelo.CorporationId, modelo.Imagen!, "Client", modelo.Active, modelo.UserType);

            if (user == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "No se pudo crear el usuario."
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

            string subject = "Activacion de Cuenta";
            string body = ($"De: NexxtPlanet" +
                $"<h1>Email Confirmation</h1>" +
                $"<p>" +
                $"Su Clave Temporal es: <h2> \"{user.Pass}\"</h2>" +
                $"</p>" +
                $"Para Activar su vuenta, " +
                $"Has Click en el siguiente Link:</br></br><strong><a href = \"{tokenLink}\">Confirmar Correo</a></strong>");

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
}
