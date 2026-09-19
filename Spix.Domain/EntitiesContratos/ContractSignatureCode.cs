using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

//Codigo de un solo uso que el cliente recibe en su correo para poder firmar.
//Es el segundo componente de identificacion (algo que tiene). Nunca se guarda el codigo en
//texto plano, solo su hash. Ver docs/Firma-Electronica-Part11.md
public class ContractSignatureCode
{
    [Key]
    public Guid ContractSignatureCodeId { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    public ContractDocumentType DocumentType { get; set; }

    //Correo al que se envio; se guarda para la evidencia
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string CodeHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public int Attempts { get; set; }

    public DateTime? UsedAt { get; set; }

    [MaxLength(64)]
    public string? RequestIp { get; set; }

    [MaxLength(512)]
    public string? RequestUserAgent { get; set; }

    public int CorporationId { get; set; }

    //Quien pidio el codigo (el propio cliente o el asesor que lo atiende)
    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }
}
