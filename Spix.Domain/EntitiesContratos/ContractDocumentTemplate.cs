using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

public class ContractDocumentTemplate
{
    [Key]
    public Guid ContractDocumentTemplateId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public ContractDocumentType DocumentType { get; set; }

    [MaxLength(120)]
    public string? FileName { get; set; }

    [MaxLength(180)]
    public string? OriginalFileName { get; set; }

    [Range(1, int.MaxValue)]
    public int PageCount { get; set; }

    public bool Active { get; set; }

    public DateTime DateCreated { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    [NotMapped]
    public string? FileFullPath { get; set; }

    [NotMapped]
    public string? FileBase64 { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<ContractDocumentTemplateField>? ContractDocumentTemplateFields { get; set; }

    public ICollection<ContractSignedDocument>? ContractSignedDocuments { get; set; }
}
