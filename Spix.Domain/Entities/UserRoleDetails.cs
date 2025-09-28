using System.ComponentModel.DataAnnotations;
using Spix.Domain.Enum;

namespace Spix.Domain.Entities;

public class UserRoleDetails
{
    [Key]
    public int UserRoleDetailsId { get; set; }

    [Display(Name = "Rol")]
    public UserType? UserType { get; set; }

    [Display(Name = "Usuario")]
    public string? UserId { get; set; }

    //Relacion
    public User? User { get; set; }
}