using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

public class ContractSignedDocument
{
    [Key]
    public Guid ContractSignedDocumentId { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    [Required]
    public Guid ContractDocumentTemplateId { get; set; }

    public ContractDocumentType DocumentType { get; set; }

    [MaxLength(120)]
    public string? FileName { get; set; }

    public bool Signed { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateSigned { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public string? UsuarioOwnerSigned { get; set; }

    public Guid? UserIdSigned { get; set; }

    //===== Evidencia de la firma electronica (ver docs/Firma-Electronica-Part11.md) =====

    //Como se verifico la identidad: portal, oficina verificada u oficina asistida
    public SignatureMethod? SignatureMethod { get; set; }

    //Correo al que se envio el codigo, y horas de envio y validacion
    [MaxLength(256)]
    public string? SignerEmail { get; set; }

    public DateTime? CodeSentAt { get; set; }

    public DateTime? CodeValidatedAt { get; set; }

    //Origen de la firma; lo entrega el backend, no el navegador
    [MaxLength(64)]
    public string? SignerIp { get; set; }

    [MaxLength(512)]
    public string? SignerUserAgent { get; set; }

    //Cuando marco "He leido el documento y estoy conforme"
    public DateTime? TermsAcceptedAt { get; set; }

    //Huella SHA-256 del documento firmado, calculada ANTES de anexar la hoja de certificado
    //(es la que se imprime en esa hoja)
    [MaxLength(128)]
    public string? DocumentHash { get; set; }

    //Huella SHA-256 del archivo final tal como quedo guardado. Es la que puede comprobar
    //cualquiera subiendo el PDF a la pagina publica de verificacion.
    [MaxLength(128)]
    public string? FileHash { get; set; }

    //Identificador publico de la firma: va impreso en el documento y en el codigo QR
    [MaxLength(32)]
    public string? VerificationCode { get; set; }

    //Version y huella del aviso de firma electronica que acepto el firmante
    [MaxLength(20)]
    public string? ConsentVersion { get; set; }

    [MaxLength(128)]
    public string? ConsentHash { get; set; }

    //Asesor testigo, solo en firma presencial asistida
    [MaxLength(256)]
    public string? WitnessUserName { get; set; }

    public Guid? WitnessUserId { get; set; }

    [NotMapped]
    public string? FileFullPath { get; set; }

    [NotMapped]
    public string? SignatureBase64 { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public ContractDocumentTemplate? ContractDocumentTemplate { get; set; }
}
