using Spix.DomainLogic.ResponcesSec;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesSecure;
using Spix.UnitOfWork.InterfacesSecure;

namespace Spix.UnitOfWork.ImplementSecure;

public class AccountUnitOfWork : IAccountUnitOfWork
{
    private readonly IAccountService _accountService;

    public AccountUnitOfWork(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<ActionResponse<TokenDTO>> LoginAsync(LoginDTO modelo) => await _accountService.LoginAsync(modelo);

    public async Task<ActionResponse<bool>> RecoverPasswordAsync(RecoveryPassDTO modelo, string frontUrl) => await _accountService.RecoverPasswordAsync(modelo, frontUrl);

    public async Task<ActionResponse<bool>> ResetPasswordAsync(ResetPasswordDTO modelo) => await _accountService.ResetPasswordAsync(modelo);

    public async Task<ActionResponse<bool>> ChangePasswordAsync(ChangePasswordDTO modelo, string UserName) => await _accountService.ChangePasswordAsync(modelo, UserName);

    public async Task<ActionResponse<bool>> ConfirmEmailAsync(string userId, string token) => await _accountService.ConfirmEmailAsync(userId, token);
}