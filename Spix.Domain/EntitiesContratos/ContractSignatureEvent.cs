using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

//Bitacora del proceso de firma: un renglon por cada paso (enviado, visto, codigo, firmado).
//Solo se escribe, nunca se edita ni se borra. Ver docs/Firma-Electronica-Part11.md
public class ContractSignatureEvent
{
    [Key]
    public Guid ContractSignatureEventId { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    public ContractDocumentType DocumentType { get; set; }

    public SignatureEventType EventType { get; set; }

    public DateTime CreatedAt { get; set; }

    //Dato corto que explica el paso: correo enmascarado, motivo del fallo, etc.
    [MaxLength(256)]
    public string? Detail { get; set; }

    //Origen del paso; lo arma el backend, no el navegador
    [MaxLength(64)]
    public string? SourceIp { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public int CorporationId { get; set; }

    //Quien ejecuto el paso: el cliente o el usuario de la oficina
    [MaxLength(256)]
    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }
}
