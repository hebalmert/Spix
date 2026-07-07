using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Spix.DomainLogic.EnumTypes;

namespace Spix.Domain.EntitiesContratos;

public class ContractDocumentTemplateField
{
    [Key]
    public Guid ContractDocumentTemplateFieldId { get; set; }

    [Required]
    public Guid ContractDocumentTemplateId { get; set; }

    public ContractDocumentFieldType FieldType { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; }

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PositionX { get; set; }

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PositionY { get; set; }

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Width { get; set; }

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Height { get; set; }

    [Range(1, 80)]
    public int FontSize { get; set; }

    public ContractDocumentTemplate? ContractDocumentTemplate { get; set; }
}
