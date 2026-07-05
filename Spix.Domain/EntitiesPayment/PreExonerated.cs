using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesPayment;

public class PreExonerated
{
    [Key]
    public Guid PreExoneratedId { get; set; }

    public DateTime DateExonerated { get; set; }

    [MaxLength(25)]
    public string? ExoneratedControl { get; set; }

    public Guid ClientId { get; set; }

    public Guid ContractClientId { get; set; }

    public Guid PlanId { get; set; }

    public int YearNumber { get; set; }

    public MonthType MonthType { get; set; }

    public decimal TaxRate { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal PriceWithTax { get; set; }

    public bool Billed { get; set; }

    public DateTime? DateBilled { get; set; }

    public Guid? CxCBillId { get; set; }

    [DisplayName("Cliente")]
    [NotMapped]
    public string? ClientFullName { get; set; }

    [NotMapped]
    public string? ContractAddress { get; set; }

    [NotMapped]
    public string? ContractPhone { get; set; }

    [NotMapped]
    public string? ContractCity { get; set; }

    [NotMapped]
    public string? ContractZone { get; set; }

    [NotMapped]
    public decimal? TaxTotal => TaxRate == 0 ? 0 : PriceWithTax - UnitPrice;

    [NotMapped]
    public decimal? TotalUnitPrice => UnitPrice;

    [NotMapped]
    public decimal? TotalWithTax => PriceWithTax;

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public Client? Client { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Plan? Plan { get; set; }

    public CxCBill? CxCBill { get; set; }
}
