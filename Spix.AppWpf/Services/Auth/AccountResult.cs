namespace Spix.AppWpf.Services.Auth;

// Expone el resultado de una accion de cuenta para que la vista no conozca HTTP.
public class AccountResult
{
    public bool WasSuccess { get; init; }

    public string? Message { get; init; }
}
