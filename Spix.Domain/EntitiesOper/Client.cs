using Spix.Core.EntitiesContratos;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Core.EntitiesOper;

public class Client
{
    [Key]
    public Guid ClientId { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Creado")]
    public DateTime? DateCreated { get; set; }

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [Display(Name = "Tipo")]
    public Guid DocumentTypeId { get; set; }

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(25, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Documento")]
    public string Document { get; set; } = null!;

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(50, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = null!;

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(50, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = null!;

    [MaxLength(101, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Nombre")]
    public string? FullName { get; set; }

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

    //[Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(256, ErrorMessage = "El campo no puede ser mayor a {0} de largo")]
    [Display(Name = "Direccion")]
    public string Address { get; set; } = null!;

    [Display(Name = "Tipo Usuario")]
    public UserType UserType { get; set; }

    [MaxLength(256, ErrorMessage = "El campo no puede ser mayor a {0} de largo")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string UserName { get; set; } = null!;

    [Display(Name = "Foto")]
    public string? Photo { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //TODO: Cambio de ruta para Imagenes
    public string ImageFullPath => Photo == string.Empty || Photo == null
        ? $"https://localhost:7224/Images/NoPicture.png"
        : $"https://localhost:7224/Images/ImgClient/{Photo}";

    [NotMapped]
    public string? ImgBase64 { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public DocumentType? DocumentType { get; set; }

    public ICollection<ContractClient>? ContractClients { get; set; }
}