using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesSecure;

public interface IAccountService
{
    Task<ActionResponse<TokenDTO>> LoginAsync(LoginDTO modelo);

    Task<ActionResponse<bool>> RecoverPasswordAsync(RecoveryPassDTO modelo, string frontUrl);

    Task<ActionResponse<bool>> ResetPasswordAsync(ResetPasswordDTO modelo);

    Task<ActionResponse<bool>> ChangePasswordAsync(ChangePasswordDTO modelo, string UserName);

    Task<ActionResponse<bool>> ConfirmEmailAsync(string userId, string token);
}