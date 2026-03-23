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
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesOper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xNotification.Interfaces;

namespace Spix.Services.ImplementOper;

public class ContractorService : IContractorService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IUserHelper _userHelper;
    private readonly IEmailHelper _emailHelper;
    private readonly IStringLocalizer _localizer;
    private readonly ImgSetting _imgOption;

    public ContractorService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, IFileStorage fileStorage, HttpErrorHandler httpErrorHandler,
        IUserHelper userHelper, IEmailHelper emailHelper, IOptions<ImgSetting> ImgOption, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _userHelper = userHelper;
        _emailHelper = emailHelper;
        _localizer = localizer;
        _imgOption = ImgOption.Value;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<IEnumerable<Contractor>>> ComboAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Contractor>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }
            var ListModel = await _context.Contractors.Where(x => x.Active && x.CorporationId == user.CorporationId).ToListAsync();

            return new ActionResponse<IEnumerable<Contractor>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Contractor>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Contractor>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Contractor>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.Contractors.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

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

            return new ActionResponse<IEnumerable<Contractor>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Contractor>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Contractor>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.Contractors.Include(x => x.DocumentType).FirstOrDefaultAsync(x => x.ContractorId == id);
            if (modelo == null)
            {
                return new ActionResponse<Contractor>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<Contractor>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Contractor>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Contractor>> UpdateAsync(Contractor modelo, string frontUrl)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            Contractor NewModelo = _mapperService.Map<Contractor, Contractor>(modelo);
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
                NewModelo.Imagen = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgContractor!);
            }
            _context.Contractors.Update(NewModelo);
            await _transactionManager.SaveChangesAsync();

            User UserCurrent = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
            if (UserCurrent != null)
            {
                UserCurrent.FirstName = modelo.FirstName;
                UserCurrent.LastName = modelo.LastName;
                UserCurrent.PhoneNumber = modelo.PhoneNumber;
                UserCurrent.PhotoUser = modelo.Imagen;
                UserCurrent.JobPosition = "Client";
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
                        _fileStorage.DeleteImage(_imgOption.ImgContractor!, guid!);
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<Contractor>
                        {
                            WasSuccess = false,
                            Message = _localizer["Generic_UserCreationFail"]
                        };
                    }
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Contractor>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Contractor>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Contractor>> AddAsync(Contractor modelo, string username, string frontUrl)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            User CheckEmail = await _userHelper.GetUserByEmailAsync(modelo.UserName);
            if (CheckEmail != null)
            {
                return new ActionResponse<Contractor>
                {
                    WasSuccess = true,
                    Message = "El Correo ingresado ya se encuentra reservado, debe cambiarlo."
                };
            }

            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Contractor>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            modelo.DateCreated = DateTime.Now;
            modelo.UserType = UserType.Contractor;
            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                modelo.Imagen = await _fileStorage.UploadImage(imageId, _imgOption.ImgContractor!, guid);
            }

            _context.Contractors.Add(modelo);
            await _transactionManager.SaveChangesAsync();

            //Registro del Usuario en User
            if (modelo.Active)
            {
                Response response = await AcivateUser(modelo, frontUrl);
                if (!response.IsSuccess)
                {
                    var guid = modelo.Imagen;
                    _fileStorage.DeleteImage(_imgOption.ImgManager!, guid!);
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<Contractor>
                    {
                        WasSuccess = true,
                        Message = "No se ha podido crear el Usuario, Intentelo de nuevo"
                    };
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Contractor>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Contractor>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Contractors.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            var user = await _userHelper.GetUserByUserNameAsync(DataRemove.UserName);
            var RemoveRoleDetail = await _context.UserRoleDetails.Where(x => x.UserId == user.Id).ToListAsync();
            if (RemoveRoleDetail != null)
            {
                _context.UserRoleDetails.RemoveRange(RemoveRoleDetail!);
            }
            await _userHelper.DeleteUser(DataRemove.UserName);

            _context.Contractors.Remove(DataRemove);

            if (DataRemove.Imagen is not null)
            {
                var response = await _fileStorage.RemoveFileAsync(_imgOption.ImgContractor!, DataRemove.Imagen);
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

    private async Task<Response> AcivateUser(Contractor modelo, string frontUrl)
    {
        User user = await _userHelper.AddUserUsuarioAsync(modelo.FirstName, modelo.LastName, modelo.UserName, modelo.Email,
            modelo.PhoneNumber, modelo.Address, "Contractor", modelo.CorporationId, modelo.Imagen!, "Contractor", modelo.Active, modelo.UserType);

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

        Response response = await _emailHelper.ConfirmarCuenta(user.UserName!, $"{user.FirstName} {user.LastName}", subject, body);
        if (response.IsSuccess == false)
        {
            return response;
        }
        return response;
    }
}