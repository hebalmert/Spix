using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

//Bitacora del contrato: un renglon por cada paso, venga del modulo que venga.
//Solo se escribe; nunca se edita ni se borra.
//
//Antes el rastro de un contrato estaba repartido (las columnas del propio contrato, la
//bitacora de firma, la suspension, las exoneraciones), y cada pantalla mostraba el pedazo
//que conocia. Aqui queda la linea de tiempo completa y hay un solo lugar que leer.
//
//ContractSignatureEvent se conserva tal cual porque es la evidencia que sostiene el
//certificado Part 11; sus pasos se anotan ademas aqui para poder verlos junto a lo demas.
public class ContractAudit
{
    [Key]
    public Guid ContractAuditId { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    public Guid ClientId { get; set; }

    public DateTime DateEvent { get; set; }

    public ContractEventType EventType { get; set; }

    //Dato corto que explica el paso: el estado al que paso, el motivo, el mes exonerado,
    //el correo enmascarado. Lo arma el modulo que anota.
    [MaxLength(256)]
    public string? Detail { get; set; }

    //El registro que origino el paso (la suspension, la exoneracion, el documento firmado),
    //para poder ir a verlo sin tener que adivinar cual era.
    public Guid? ReferenceId { get; set; }

    //Origen del paso cuando lo hace el cliente desde su portal; lo arma el backend
    [MaxLength(64)]
    public string? SourceIp { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    //Quien lo hizo: el usuario de la oficina o el propio cliente
    public Guid? UserId { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Client? Client { get; set; }
}
