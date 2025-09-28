namespace Spix.AppFront.AuthenticationProviders;

public interface ILoginService
{
    Task LoginAsync(string token);

    Task LogoutAsync();
}