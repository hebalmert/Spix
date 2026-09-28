using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

//La credencial PPPoE de un contrato: el gemelo de ContractBind, pero para /ppp/secret.
//
//Misma forma que su hermano a proposito: un MikrotikId de 15 caracteres con el .id del
//equipo, y una foto denormalizada de lo que se le escribio, para poder reescribirlo sin
//tener que rearmar todo el grafo de navegaciones.
//
//NO lleva CargueDetailId (la MAC): en PPPoE el corte es por usuario y clave, la MAC no
//corta nada. WISPHUB tampoco la pide para dar de alta un cliente PPPoE.
public class ContractPppoe
{
    [Key]
    public Guid ContractPppoeId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Servidor")]
    public Guid ServerId { get; set; }

    //La IP fija del cliente: viaja al equipo como remote-address del secret. Es lo que
    //mantiene funcionando el sistema de Queues con la tasa de reuso sin cambiarle nada.
    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Ip Cliente")]
    public Guid IpNetId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [MaxLength(50, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    //Un usuario PPPoE tiene que ser algo: con el numero del contrato pelado quedaba "1".
    //Minimo 6, con letras y numeros, y solo lo que MikroTik acepta sin problemas.
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9._-]{6,50}$",
        ErrorMessageResourceName = nameof(Resource.Validation_PppoeUser), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Usuario PPPoE")]
    public string Usuario { get; set; } = null!;

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [MaxLength(50, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [Display(Name = "Clave PPPoE")]
    public string Clave { get; set; } = null!;

    [Display(Name = "Estado Acceso")]
    public PppoeAccessState PppoeAccessState { get; set; } = PppoeAccessState.Activo;

    [MaxLength(15, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [Display(Name = "Mikrotik Id")]
    public string? MikrotikId { get; set; }

    //Lo que quedo escrito en el MikroTik, para poder reescribirlo igual

    [MaxLength(100)]
    [Display(Name = "Servidor")]
    public string? ServerName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Servidor IP")]
    public string? IpServer { get; set; }

    [MaxLength(100)]
    [Display(Name = "Ip Cliente")]
    public string? IpCliente { get; set; }

    [MaxLength(50)]
    [Display(Name = "Perfil PPPoE")]
    public string? ProfileName { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual Server? Server { get; set; }

    public virtual IpNet? IpNet { get; set; }
}
