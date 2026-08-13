using Spix.DomainLogic.AppResponses;
using Spix.HttpService;
using System.Net.Http;

namespace Spix.AppWpf.Services.Auth;

// Consume los endpoints protegidos de cuenta usando el JWT centralizado.
public class AccountService : IAccountService
{
    private const string ChangePasswordUrl = "api/v1/accounts/changePassword";

    private readonly IRepository _repository;

    public AccountService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountResult> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
    {
        try
        {
            var responseHttp = await _repository.PostAsync(
                ChangePasswordUrl,
                changePasswordDTO);

            if (!responseHttp.Error)
            {
                return new AccountResult
                {
                    WasSuccess = true,
                    Message = "La clave fue actualizada correctamente."
                };
            }

            var message = await responseHttp.GetErrorMessageAsync();
            return new AccountResult
            {
                WasSuccess = false,
                Message = string.IsNullOrWhiteSpace(message)
                    ? "No fue posible actualizar la clave."
                    : message.Trim().Trim('"')
            };
        }
        catch (HttpRequestException)
        {
            return new AccountResult
            {
                WasSuccess = false,
                Message = "No fue posible conectar con el Backend."
            };
        }
        catch (Exception)
        {
            return new AccountResult
            {
                WasSuccess = false,
                Message = "Ocurrio un error al actualizar la clave."
            };
        }
    }
}
