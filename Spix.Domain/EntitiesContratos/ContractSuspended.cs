using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

//Registro de cada suspension de un contrato, venga de una persona o del corte masivo.
//La fila NO se borra al reactivar: se cierra con DateReactivated y queda como historia.
//Asi se puede consultar cuantos hay suspendidos hoy, cuanto suman, quienes lo estuvieron
//entre dos fechas y cuales fueron los del ultimo corte, sin castigar la tabla de contratos.
public class ContractSuspended
{
    [Key]
    public Guid ContractSuspendedId { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    public DateTime DateSuspended { get; set; }

    //Nulo = el contrato sigue suspendido
    public DateTime? DateReactivated { get; set; }

    public SuspendedOrigin Origin { get; set; }

    //Corte que la genero, cuando viene de RunSuspended
    public Guid? RunSuspendedId { get; set; }

    [MaxLength(200)]
    public string? Motivo { get; set; }

    //===== Foto del momento de la suspension =====
    //Se guardan como columnas propias a proposito: los reportes no tienen que ir a las otras
    //tablas, y si manana el plan sube de precio o el cliente cambia de direccion, el registro
    //sigue mostrando lo que habia el dia que se suspendio.

    public long ControlContrato { get; set; }

    [MaxLength(200)]
    public string? ClientName { get; set; }

    [MaxLength(50)]
    public string? ClientDocument { get; set; }

    [MaxLength(200)]
    public string? ContractAddress { get; set; }

    [MaxLength(50)]
    public string? ContractPhone { get; set; }

    [MaxLength(100)]
    public string? CityName { get; set; }

    [MaxLength(100)]
    public string? ZoneName { get; set; }

    [MaxLength(100)]
    public string? PlanName { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PlanAmount { get; set; }

    //===== Con que se edita el registro en el MikroTik =====
    //Se guardan al suspender para que este modulo pueda devolver el acceso por si solo,
    //sin depender de lo que tenga el IpBinding mas adelante.

    //El .id del ip-binding dentro del RouterOS
    [MaxLength(30)]
    public string? MkIndex { get; set; }

    //A que servidor hay que conectarse para editarlo
    public Guid? ServerId { get; set; }

    //Quien suspendio y quien reactivo
    public Guid? UserId { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    public Guid? UserIdReactivated { get; set; }

    [MaxLength(150)]
    public string? UserByNameReactivated { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Client? Client { get; set; }

    public RunSuspended? RunSuspended { get; set; }
}
