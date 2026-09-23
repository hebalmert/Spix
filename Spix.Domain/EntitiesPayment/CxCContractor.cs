using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

//La cuenta por pagar de un contratista: agrupa las comisiones que se le causaron y contra
//ella se le hacen los pagos, completos o por partes. Es el espejo de la cuenta por cobrar.
public class CxCContractor
{
    [Key]
    public Guid CxCContractorId { get; set; }

    public DateTime DateNote { get; set; }

    [MaxLength(25)]
    public string? NoteNumber { get; set; }

    public Guid ContractorId { get; set; }

    [MaxLength(250)]
    public string? Description { get; set; }

    //Lo que se le debe y lo que falta por pagarle
    public decimal Total { get; set; }

    public decimal Balance { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    public bool Cancelled { get; set; }

    public DateTime? DateCancelled { get; set; }

    [MaxLength(250)]
    public string? DescriptionCancelled { get; set; }

    public int CorporationId { get; set; }

    [MaxLength(150)]
    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public Contractor? Contractor { get; set; }

    //De que se compone
    public ICollection<ContractorAccountPayable>? ContractorAccountPayables { get; set; }

    //Lo que se le ha ido pagando
    public ICollection<CxCContractorDetail>? CxCContractorDetails { get; set; }
}
