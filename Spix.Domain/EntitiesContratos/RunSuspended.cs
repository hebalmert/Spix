using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class RunSuspended
{
    [Key]
    public Guid RunSuspendedId { get; set; }

    public int YearNumber { get; set; }

    public MonthType MonthType { get; set; }

    public bool Executed { get; set; }

    public DateTime? DateUtc { get; set; }

    public Guid? UserId { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<RunSuspendedDetail>? RunSuspendedDetails { get; set; }
}
