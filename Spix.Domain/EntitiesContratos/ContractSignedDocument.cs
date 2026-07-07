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

    [NotMapped]
    public string? FileFullPath { get; set; }

    [NotMapped]
    public string? SignatureBase64 { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public ContractDocumentTemplate? ContractDocumentTemplate { get; set; }
}
