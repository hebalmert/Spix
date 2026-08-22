using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSecure;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private const string RefreshCookie = "spix.refreshToken";
        private readonly IAccountServiceX _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public AccountsController(IAccountServiceX accountUnitOfWork,
            IConfiguration configuration, IStringLocalizer localizer)
        {
            _unitOfWork = accountUnitOfWork;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO modelo)
        {
            try
            {
                var response = await _unitOfWork.LoginAsync(modelo);
                if (response.WasSuccess) { var refresh = await _unitOfWork.CreateRefreshTokenAsync(modelo.UserName); if (!refresh.WasSuccess) return ResponseHelper.Format(refresh); Response.Cookies.Append(RefreshCookie, refresh.Result!, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Secure = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(), Path = "/api/v1/accounts" }); }
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
            }
        }

        [HttpPost("RecoverPassword")]
        public async Task<IActionResult> RecoverPasswordAsync([FromBody] RecoveryPassDTO modelo)
        {
            try
            {
                var response = await _unitOfWork.RecoverPasswordAsync(modelo, _configuration["UrlFrontend"]!);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDTO modelo)
        {
            try
            {
                var response = await _unitOfWork.ResetPasswordAsync(modelo);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
            }
        }

        [HttpPost("changePassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDTO modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["Generic_InvalidModel"]);
            }
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

            try
            {
                var response = await _unitOfWork.ChangePasswordAsync(modelo, userClaimsInfo.UserName);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
            }
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                token = token.Replace(" ", "+");
                var response = await _unitOfWork.ConfirmEmailAsync(userId, token);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
            }
        }

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshTokenAsync() { var response = await _unitOfWork.RefreshTokenAsync(Request.Cookies[RefreshCookie] ?? string.Empty); if (!response.WasSuccess || response.Result is null) return ResponseHelper.Format(response); Response.Cookies.Append(RefreshCookie, response.Result.RefreshToken, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Secure = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(), Path = "/api/v1/accounts" }); return Ok(response.Result.AccessToken); }

        [HttpPost("Logout")]
        [AllowAnonymous]
        public async Task<IActionResult> LogoutAsync() { await _unitOfWork.RevokeRefreshTokenAsync(Request.Cookies[RefreshCookie] ?? string.Empty); Response.Cookies.Delete(RefreshCookie, new CookieOptions { Path = "/api/v1/accounts" }); return NoContent(); }
    }
}
