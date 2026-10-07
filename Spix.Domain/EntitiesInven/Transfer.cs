using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Spix.DomainLogic.Validations;

using Spix.Domain.EntitiesOper;
using Spix.Domain.EntitesSoftSec;
namespace Spix.Domain.EntitiesInven;

public class Transfer
{
    [Key]
    public Guid TransferId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.TransferDate), ResourceType = typeof(Resource))]
    public DateTime DateTransfer { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.TransferNumber), ResourceType = typeof(Resource))]
    public int NroTransfer { get; set; }

    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    public string? UserId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.FromStorage), ResourceType = typeof(Resource))]
    public string? FromStorageName { get; set; }

    [ComboRequired]
    public Guid FromProductStorageId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.ToStorage), ResourceType = typeof(Resource))]
    public string? ToStorageName { get; set; }

    [ComboRequired]
    public Guid ToProductStorageId { get; set; }

    [Display(Name = nameof(Resource.Status), ResourceType = typeof(Resource))]
    public TransferType? Status { get; set; }

    [NotMapped]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public string? NombreUsuario { get; set; }

    //===== Auditoria: quien hizo que y cuando =====
    //La pone el SERVIDOR al crear y nadie la edita. DateTransfer sigue siendo la fecha
    //del movimiento, que el operador si puede cambiar; esta es la del registro.
    public DateTime? DateCreated { get; set; }

    public string? UserIdClosed { get; set; }

    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    public string? NombreUsuarioCierre { get; set; }

    public DateTime? DateClosed { get; set; }

    //===== Quien recibe los equipos =====
    //Se llena UNA de las dos: o un tecnico o un usuario del sistema.
    public Guid? ReceivedByTechnicianId { get; set; }

    public Guid? ReceivedByUsuarioId { get; set; }

    //El nombre se CONGELA al guardar, como FromStorageName: si mañana
    //se retira el tecnico, el traslado viejo sigue diciendo quien recibio.
    [MaxLength(120)]
    public string? ReceivedByName { get; set; }

    //La llave que manda el formulario: un solo combo para las dos listas.
    //Viene como "T:<guid>" si es tecnico o "U:<guid>" si es usuario; el Service la
    //reparte en las dos columnas de arriba. No es columna de la base.
    [NotMapped]
    public string? ReceiverKey { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public User? User { get; set; }
    public Technician? ReceivedByTechnician { get; set; }
    public Usuario? ReceivedByUsuario { get; set; }
    public ICollection<TransferDetails>? TransferDetails { get; set; }

}