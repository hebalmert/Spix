using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitesSoftSec;

public class UsuarioRole
{
    [Key]
    public Guid UsuarioRoleId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    public Guid UsuarioId { get; set; }

    [Display(Name = nameof(Resource.RoleUser), ResourceType = typeof(Resource))]
    public UserType UserType { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Usuario? Usuario { get; set; }
}