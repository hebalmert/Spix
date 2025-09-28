using System.ComponentModel.DataAnnotations;
using Spix.Domain.Entities;
using Spix.Domain.Enum;
using Spix.Domain.Resources;

namespace Spix.Domain.EntitesSoftSec;

public class UsuarioRole
{
    [Key]
    public int UsuarioRoleId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Usuario")]
    public int UsuarioId { get; set; }

    [Display(Name = "Role")]
    public UserType UserType { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Usuario? Usuario { get; set; }
}