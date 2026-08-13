using Spix.DomainLogic.AppResponses;

namespace Spix.AppWpf.Services.Auth;

// Expone acciones autenticadas de la cuenta que comparte el usuario desktop.
public interface IAccountService
{
    Task<AccountResult> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO);
}
