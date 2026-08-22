using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesSecure;

public interface IAccountServiceX
{
    Task<ActionResponse<TokenDTO>> LoginAsync(LoginDTO modelo);
    Task<ActionResponse<string>> CreateRefreshTokenAsync(string userName);
    Task<ActionResponse<RefreshSessionDTO>> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);

    Task<ActionResponse<bool>> RecoverPasswordAsync(RecoveryPassDTO modelo, string frontUrl);

    Task<ActionResponse<bool>> ResetPasswordAsync(ResetPasswordDTO modelo);

    Task<ActionResponse<bool>> ChangePasswordAsync(ChangePasswordDTO modelo, string UserName);

    Task<ActionResponse<bool>> ConfirmEmailAsync(string userId, string token);
}
