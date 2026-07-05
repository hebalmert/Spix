using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesBilling;

public class BillingNoteOne
{
    [Key]
    public Guid BillingNoteOneId { get; set; }

    public DateTime DateBill { get; set; }

    public Guid ClientId { get; set; }

    public Guid ContractClientId { get; set; }

    public int YearNumber { get; set; }

    public MonthType MonthType { get; set; }

    public bool Created { get; set; }

    public DateTime? DateCreated { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public Client? Client { get; set; }

    public ContractClient? ContractClient { get; set; }

    public ICollection<Sell>? Sells { get; set; }

    public ICollection<CxCBill>? CxCBills { get; set; }
}
