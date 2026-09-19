using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesSecure;

public interface IUserNameServiceX
{
    Task<ActionResponse<UserNameCheckDTO>> CheckAsync(string userName);

    Task<ActionResponse<UserNameCheckDTO>> SuggestAsync(string firstName, string lastName);
}
