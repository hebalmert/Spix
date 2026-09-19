using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.InterfacesSecure;
using Spix.DomainLogic.ModelUtility;

namespace Spix.ServiceX.ImplementSecure;

public class UserNameServiceX : IUserNameServiceX
{
    private readonly IUserNameService _userNameService;

    public UserNameServiceX(IUserNameService userNameService)
    {
        _userNameService = userNameService;
    }

    public async Task<ActionResponse<UserNameCheckDTO>> CheckAsync(string userName) =>
        await _userNameService.CheckAsync(userName);

    public async Task<ActionResponse<UserNameCheckDTO>> SuggestAsync(string firstName, string lastName) =>
        await _userNameService.SuggestAsync(firstName, lastName);
}
