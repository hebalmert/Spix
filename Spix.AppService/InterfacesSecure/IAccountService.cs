using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesSecure;

public interface IAccountService
{
    //desdeEscritorio lo manda solo el login del software de PC (v2)
    Task<ActionResponse<TokenDTO>> LoginAsync(LoginDTO modelo, bool desdeEscritorio = false);
    Task<ActionResponse<string>> CreateRefreshTokenAsync(string userName);
    Task<ActionResponse<RefreshSessionDTO>> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);

    Task<ActionResponse<bool>> RecoverPasswordAsync(RecoveryPassDTO modelo, string frontUrl);

    Task<ActionResponse<bool>> ResetPasswordAsync(ResetPasswordDTO modelo);

    Task<ActionResponse<bool>> ChangePasswordAsync(ChangePasswordDTO modelo, string UserName);

    Task<ActionResponse<bool>> ConfirmEmailAsync(string userId, string token);
}
