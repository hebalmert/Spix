using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesSaaS;

/// <summary>
/// Configuración global del SaaS guardada cifrada en la base de datos.
/// Las llaves usan el formato jerárquico de appsettings,
/// por ejemplo: "SendGrid:SendGridApiKey".
/// </summary>
public class SystemSetting
{
    [Key]
    public int SystemSettingId { get; set; }

    [Required]
    [MaxLength(120)]
    public string Key { get; set; } = null!;

    [Required]
    public string Value { get; set; } = null!;

    [MaxLength(60)]
    public string? Category { get; set; }

    public bool IsSecret { get; set; }

    public DateTime UpdatedAt { get; set; }

    [MaxLength(60)]
    public string? UpdatedBy { get; set; }
}
