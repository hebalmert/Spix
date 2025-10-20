using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.EntitiesDTO;

public class StateDTO
{
    [Key]
    public int StateId { get; set; }

    public int CountryId { get; set; }

    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Depart/Estado")]
    public string Name { get; set; } = null!;

    //Relaciones
    public CountryDTO? Country { get; set; }

    public ICollection<CityDTO>? Cities { get; set; }
}