using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Spix.AppInfra;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesSecure;
using Spix.Domain.Entities;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xLanguage.Resources;
using Spix.xNotification.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Spix.Services.ImplementSecure;

public class AccountService : IAccountService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly IEmailHelper _emailHelper;
    private readonly IStringLocalizer _localizer;
    private readonly IFileStorage _fileStorage;
    private readonly JwtKeySetting _jwtOption;
    private readonly ImgSetting _imgOption;

    public AccountService(DataContext context, IUserHelper userHelper,
        IEmailHelper emailHelper, IOptions<ImgSetting> ImgOption,
        IOptions<JwtKeySetting> jwtOption, IStringLocalizer localizer,
        IFileStorage fileStorage)
    {
        _context = context;
        _userHelper = userHelper;
        _emailHelper = emailHelper;
        _localizer = localizer;
        _fileStorage = fileStorage;
        _jwtOption = jwtOption.Value;
        _imgOption = ImgOption.Value;
    }

    public async Task<ActionResponse<TokenDTO>> LoginAsync(LoginDTO modelo)
    {
        string? imgUsuario = string.Empty;
        string? ImagenDefault = _imgOption.ImgNoImage;
        string BaseUrl = _imgOption.ImgBaseUrl;

        var result = await _userHelper.LoginAsync(modelo);
        if (result.Succeeded)
        {
            //Consulto User de IdentityUser
            var user = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
            if (!user.Active)
            {
                return new ActionResponse<TokenDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_UserInactive)]
                };
            }
            var RolesUsuario = _context.UserRoleDetails.Where(c => c.UserId == user.Id).ToList();
            if (RolesUsuario.Count == 0)
            {
                return new ActionResponse<TokenDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_UserNoRoleAssigned)]
                };
            }
            var RolUsuario = RolesUsuario.FirstOrDefault(c => c.UserType == UserType.Admin);
            if (RolUsuario == null)
            {
                var CheckCorporation = await _context.Corporations.FirstOrDefaultAsync(x => x.CorporationId == user.CorporationId);
                DateTime hoy = DateTime.Today;
                DateTime current = CheckCorporation!.DateEnd;
                if (!CheckCorporation.Active)
                {
                    return new ActionResponse<TokenDTO>
                    {
                        WasSuccess = false,
                        Message = _localizer[nameof(Resource.Generic_CorporationInactive)]
                    };
                }
                if (current <= hoy)
                {
                    return new ActionResponse<TokenDTO>
                    {
                        WasSuccess = false,
                        Message = _localizer[nameof(Resource.Generic_PlanExpired)]
                    };
                }

                switch (user.UserFrom)
                {
                    case "Manager":
                        if (!string.IsNullOrWhiteSpace(user.PhotoUser))
                        {
                            var FileResult = await _fileStorage.GetFileBase64Async(user.PhotoUser, _imgOption.ImgManager);
                            imgUsuario = FileResult!.Base64;
                        }
                        else
                        {
                            imgUsuario = ImagenDefault;
                        }
                        break;

                    case "UsuarioSoftware":
                        if (!string.IsNullOrWhiteSpace(user.PhotoUser))
                        {
                            var FileResult = await _fileStorage.GetFileBase64Async(user.PhotoUser, _imgOption.ImgUsuario);
                            imgUsuario = FileResult!.Base64;
                        }
                        else
                        {
                            imgUsuario = ImagenDefault;
                        }
                        break;

                    case "Patient":
                        imgUsuario = ImagenDefault;
                        break;
                }
            }
            return new ActionResponse<TokenDTO>
            {
                WasSuccess = true,
                Result = await BuildToken(user, imgUsuario!)
            };
        }

        if (result.IsLockedOut)
        {
            return new ActionResponse<TokenDTO>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_UserBlocked)]
            };
        }

        if (result.IsNotAllowed)
        {
            return new ActionResponse<TokenDTO>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_AccessDenied)]
            };
        }

        return new ActionResponse<TokenDTO>
        {
            WasSuccess = false,
            Message = _localizer[nameof(Resource.Generic_InvalidCredentials)]
        };
    }

    public async Task<ActionResponse<bool>> RecoverPasswordAsync(RecoveryPassDTO modelo, string frontUrl)
    {
        var CheckEmail = await _userHelper.GetUserByEmailAsync(modelo.Email);
        if (CheckEmail == null)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_RegisterNotFound)]
            };
        }

        var user = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
        if (user == null)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_RegisterNotFound)]
            };
        }

        if (user.Id != CheckEmail.Id)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_IdNotFound)]
            };
        }

        Response response = await SendRecoverEmailAsync(user, frontUrl);
        if (response.IsSuccess)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = response.Message
            };
        }

        return new ActionResponse<bool>
        {
            WasSuccess = false,
            Message = response.Message
        };
    }

    public async Task<ActionResponse<bool>> ResetPasswordAsync(ResetPasswordDTO modelo)
    {
        var userEmail = await _userHelper.GetUserByEmailAsync(modelo.Email);
        if (userEmail == null)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_UserFail)]
            };
        }

        var user = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
        if (user == null)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_UserFail)]
            };
        }

        if (user.Id != userEmail.Id)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_UserFail)]
            };
        }

        var result = await _userHelper.ResetPasswordAsync(user, modelo.Token, modelo.NewPassword);
        if (result.Succeeded)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        return new ActionResponse<bool>
        {
            WasSuccess = false,
            Message = result.Errors.FirstOrDefault()!.Description
        };
    }

    public async Task<ActionResponse<bool>> ChangePasswordAsync(ChangePasswordDTO modelo, string UserName)
    {
        var user = await _userHelper.GetUserByUserNameAsync(UserName);
        if (user == null)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_UserFail)]
            };
        }

        var result = await _userHelper.ChangePasswordAsync(user, modelo.CurrentPassword, modelo.NewPassword);
        if (!result.Succeeded)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
            };
        }

        return new ActionResponse<bool>
        {
            WasSuccess = true,
            Message = _localizer[nameof(Resource.Generic_Success)]
        };
    }

    public async Task<ActionResponse<bool>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userHelper.GetUserByIdAsync(new Guid(userId));
        if (user == null)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_UserFail)]
            };
        }

        var result = await _userHelper.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
            };
        }

        return new ActionResponse<bool>
        {
            WasSuccess = true,
            Message = _localizer[nameof(Resource.Generic_Success)]
        };
    }

    private async Task<Response> SendRecoverEmailAsync(User user, string frontUrl)
    {
        var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(myToken);
        // Construir la URL sin `Url.Action`
        string tokenLink = $"{frontUrl}/api/accounts/ResetPassword?token={encodedToken}";

        string subject = _localizer[nameof(Resource.Password_Recovery)];
        string body = ($"De: NexxtPlanet" +
            $"<h1>Para Recuperar su Clave</h1>" +
            $"<p>" +
            $"Para Crear una clave nueva " +
            $"Has Click en el siguiente Link:</br></br><strong><a href = \"{tokenLink}\">Cambiar Clave</a></strong>");

        Response response = await _emailHelper.ConfirmarCuenta(user.Email!, $"{user.FirstName} {user.LastName}", subject, body);
        if (response.IsSuccess == false)
        {
            return response;
        }
        return response;
    }

    private async Task<TokenDTO> BuildToken(User user, string? imgUsuario)
    {
        string NomCompa;
        string? LogoCompa;
        var RolesUsuario = _context.UserRoleDetails.Where(c => c.UserId == user.Id).ToList();
        var RolUsuario = RolesUsuario.Where(c => c.UserType == UserType.Admin).FirstOrDefault();
        if (RolUsuario != null)
        {
            //TODO: Cambio de Path para Imagenes
            NomCompa = "Nexxtplanet";
            LogoCompa = _imgOption.LogoSoftware;
            imgUsuario = _imgOption.LogoSoftware;
        }
        else
        {
            var compname = _context.Corporations.FirstOrDefault(x => x.CorporationId == user.CorporationId);
            NomCompa = compname!.Name!;
            if (!string.IsNullOrWhiteSpace(compname.Imagen))
            {
                var FileResult = await _fileStorage.GetFileBase64Async(compname.Imagen, _imgOption.ImgCorporation);
                LogoCompa = FileResult!.Base64;
            }
            else
            {
                LogoCompa = _imgOption.LogoSoftware;
            }
        }
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("CorpName", NomCompa)
            };
        // Solo agregar el CorporateId si el usuario NO es Admin
        if (RolUsuario == null && user.CorporationId.HasValue)
        {
            claims.Add(new Claim("CorporateId", user.CorporationId.Value.ToString()));
        }

        // Agregar los roles del usuario a los claims
        foreach (var item in RolesUsuario)
        {
            claims.Add(new Claim(ClaimTypes.Role, item.UserType.ToString()!));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.jwtKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddDays(7);
        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new TokenDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            PhotoBase64 = imgUsuario,
            LogoBase64 = LogoCompa
        };
    }
}