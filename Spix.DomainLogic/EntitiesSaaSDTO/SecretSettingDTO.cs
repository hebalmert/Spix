namespace Spix.DomainLogic.EntitiesSaaSDTO;

/// <summary>
/// Dato de configuración para el panel SaaS.
/// Los valores secretos nunca se devuelven al navegador.
/// </summary>
public class SecretSettingDTO
{
    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string Category { get; set; } = null!;

    public bool IsSecret { get; set; }

    public bool HasValue { get; set; }

    public string? Value { get; set; }
}
