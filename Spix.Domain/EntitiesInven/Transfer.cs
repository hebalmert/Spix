using Spix.Domain.Entities;
using Spix.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Transfer
{
    [Key]
    public Guid TransferId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Fecha de Compra")]
    public DateTime DateTransfer { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Tranferencia#")]
    public int NroTransfer { get; set; }

    [Display(Name = "Usuario")]
    public string? UserId { get; set; }

    [MaxLength(50, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = "Desde")]
    public string? FromStorageName { get; set; }

    public Guid FromProductStorageId { get; set; }

    [MaxLength(50, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = "Hacia")]
    public string? ToStorageName { get; set; }

    public Guid ToProductStorageId { get; set; }

    [Display(Name = "Estado")]
    public TransferType? Status { get; set; }

    [NotMapped]
    [Display(Name = "Nombre")]
    public string? NombreUsuario { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public User? User { get; set; }

    public ICollection<TransferDetails>? TransferDetails { get; set; }
}