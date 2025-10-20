using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.EntitiesDTO;

public class CountryDTO
{
    [Key]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es obligatorio")]
    [MaxLength(100, ErrorMessage = "El Campo {0} no puede ser mayor de {1} Caracteres")]
    [Display(Name = "Pais")]
    public string Name { get; set; } = null!;

    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener mas de {1} Caracter")]
    [Display(Name = "Cod Phone")]
    public string? CodPhone { get; set; }

    //relaciones
    public ICollection<StateDTO>? States { get; set; }
}