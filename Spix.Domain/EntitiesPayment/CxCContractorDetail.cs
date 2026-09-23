using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

//Cada pago que se le hace al contratista contra su cuenta: puede ser completo o un abono
public class CxCContractorDetail
{
    [Key]
    public Guid CxCContractorDetailId { get; set; }

    public Guid CxCContractorId { get; set; }

    public DateTime DatePayment { get; set; }

    [MaxLength(30)]
    public string? PaymentMode { get; set; }

    [MaxLength(50)]
    public string? Reference { get; set; }

    [MaxLength(256)]
    public string? Detail { get; set; }

    //Como quedo la cuenta con este pago
    public decimal Debt { get; set; }

    public decimal Payment { get; set; }

    public decimal Balance { get; set; }

    public int CorporationId { get; set; }

    [MaxLength(150)]
    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public CxCContractor? CxCContractor { get; set; }
}
