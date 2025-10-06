using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Supplier
{
    [Key]
    public Guid SupplierId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = "Nombre del Proveedor")]
    public string Name { get; set; } = null!;

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [Display(Name = "Tipo")]
    public Guid DocumentTypeId { get; set; }

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(25, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Documento")]
    public string Document { get; set; } = null!;

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(6, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Pais")]
    public string CodeCountry { get; set; } = null!;

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(3, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Codigo")]
    public string CodeNumber { get; set; } = null!;

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(7, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Telefono")]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [MaxLength(256, ErrorMessage = "El campo no puede ser mayor a {0} de largo")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string UserName { get; set; } = null!;

    [MaxLength(100, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = "Contacto")]
    public string? ContactName { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [Display(Name = "Departamento")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [Display(Name = "Ciudad")]
    public int CityId { get; set; }

    [Display(Name = "Foto")]
    public string? Photo { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //TODO: Pending to put the correct paths
    [Display(Name = "Foto")]
    public string ImageFullPath => Photo == string.Empty || Photo == null
    ? $"https://localhost:7224/Images/NoImage.png"
    : $"https://localhost:7224/Images/ImgSuppliers/{Photo}";

    [NotMapped]
    public string? ImgBase64 { get; set; }

    //A que Corporacion Pertenece

    public int CorporationId { get; set; }
    public Corporation? Corporation { get; set; }

    // Relación

    public State? State { get; set; }
    public City? City { get; set; }
    public DocumentType? DocumentType { get; set; }
    public ICollection<Purchase>? Purchases { get; set; }
}