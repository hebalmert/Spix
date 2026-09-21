using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

//Registro de cada exoneracion de un contrato. La fila NO se borra al retirar el beneficio:
//se cierra con DateEnded y queda como historia. Asi se puede consultar cuantos contratos
//estan exonerados hoy, cuanto se deja de cobrar por ellos y quienes lo estuvieron entre
//dos fechas, sin castigar la tabla de contratos.
//
//Exonerar NO toca el MikroTik: el cliente sigue con su servicio igual que un contrato
//activo, lo unico que cambia es el estado del contrato.
public class ContractExempt
{
    [Key]
    public Guid ContractExemptId { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    public DateTime DateExempt { get; set; }

    //Nulo = el contrato sigue exonerado
    public DateTime? DateEnded { get; set; }

    [MaxLength(200)]
    public string? Motivo { get; set; }

    //===== Foto del momento de la exoneracion =====
    //Se guardan como columnas propias a proposito: los reportes no tienen que ir a las otras
    //tablas, y si manana el plan sube de precio o el cliente cambia de direccion, el registro
    //sigue mostrando lo que habia el dia que se exonero.

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

    //Quien exonero y quien retiro el beneficio
    public Guid? UserId { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    public Guid? UserIdEnded { get; set; }

    [MaxLength(150)]
    public string? UserByNameEnded { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Client? Client { get; set; }
}
