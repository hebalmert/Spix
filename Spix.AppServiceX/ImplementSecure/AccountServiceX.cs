using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.InterfacesSecure;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ModelUtility;

namespace Spix.UnitOfWork.ImplementSecure;

public class AccountServiceX : IAccountServiceX
{
    private readonly IAccountService _accountService;

    public AccountServiceX(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<ActionResponse<TokenDTO>> LoginAsync(LoginDTO modelo) => await _accountService.LoginAsync(modelo);

    public async Task<ActionResponse<bool>> RecoverPasswordAsync(RecoveryPassDTO modelo, string frontUrl) => await _accountService.RecoverPasswordAsync(modelo, frontUrl);

    public async Task<ActionResponse<bool>> ResetPasswordAsync(ResetPasswordDTO modelo) => await _accountService.ResetPasswordAsync(modelo);

    public async Task<ActionResponse<bool>> ChangePasswordAsync(ChangePasswordDTO modelo, string UserName) => await _accountService.ChangePasswordAsync(modelo, UserName);

    public async Task<ActionResponse<bool>> ConfirmEmailAsync(string userId, string token) => await _accountService.ConfirmEmailAsync(userId, token);
}