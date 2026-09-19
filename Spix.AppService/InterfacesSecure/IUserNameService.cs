using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesSecure;

public interface IUserNameService
{
    //Revisa si el nombre de usuario esta libre en Identity; si no, devuelve alternativas
    Task<ActionResponse<UserNameCheckDTO>> CheckAsync(string userName);

    //Propone un usuario a partir del nombre y el apellido (nombre + inicial del apellido)
    Task<ActionResponse<UserNameCheckDTO>> SuggestAsync(string firstName, string lastName);
}
