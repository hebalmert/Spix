using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class ContractorPayment
{
    [Key]
    public Guid ContractorPaymentId { get; set; }

    public DateTime DatePayment { get; set; }

    [MaxLength(20)]
    public string PaymentNumber { get; set; } = null!;

    public Guid ContractorId { get; set; }

    [MaxLength(30)]
    public string PaymentMode { get; set; } = null!;

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(250)]
    public string? Detail { get; set; }

    public decimal Total { get; set; }

    public int CorporationId { get; set; }

    [MaxLength(150)]
    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Contractor? Contractor { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<ContractorPaymentDetail>? ContractorPaymentDetails { get; set; }
}
