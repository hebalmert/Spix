using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesBilling;

public class BillingNote
{
    [Key]
    public Guid BillingNoteId { get; set; }

    public DateTime DateBill { get; set; }

    public int YearNumber { get; set; }

    public MonthType MonthType { get; set; }

    public bool Created { get; set; }

    public DateTime? DateCreated { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Sell>? Sells { get; set; }
}
