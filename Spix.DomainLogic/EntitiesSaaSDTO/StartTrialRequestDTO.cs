using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.EntitiesSaaSDTO;

public class StartTrialRequestDTO
{
    [Required]
    [MaxLength(100)]
    public string CorporationName { get; set; } = null!;
    [Required]
    [MaxLength(25)]
    public string CorporationDocument { get; set; } = null!;
    [Required]
    [MaxLength(25)]
    public string CorporationPhone { get; set; } = null!;
    [Required]
    [MaxLength(256)]
    public string CorporationAddress { get; set; } = null!;
    [Required]
    public int SoftPlanId { get; set; }
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    [Required]
    [MaxLength(25)]
    public string Document { get; set; } = null!;
    [Required]
    [MaxLength(25)]
    public string Phone { get; set; } = null!;
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;
    [Required]
    [StringLength(24, MinimumLength = 6)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$")]
    public string UserName { get; set; } = null!;
}
