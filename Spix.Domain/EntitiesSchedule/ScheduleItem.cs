using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesSchedule;

public class ScheduleItem
{
    [Key]
    public Guid ScheduleItemId { get; set; }

    // Título y descripción
    [MaxLength(100)]
    [Required]
    public string Title { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Rango de tiempo
    [Required]
    public DateTime StartUtc { get; set; }

    [Required]
    public DateTime EndUtc { get; set; }

    public bool IsAllDay { get; set; }

    // Zona horaria (opcional)
    [MaxLength(100)]
    public string? TimeZoneId { get; set; }

    // Usuario al que se le asigna la tarea (Spix.Usuario)
    [Required]
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    //Cual fue el usuario que Creo el Registro
    [Display(Name = "Quien Creo el QcGeneral")]  //El usuario que creo el QC su nombre en base al logueo del usuario.
    public string? UsuarioOwner { get; set; }

    [Display(Name = "Quien Creo el QcGeneral")]  //El UserId del usuario que lo creo para ubicarle sus credenciales.
    public Guid? UserId { get; set; }

    // Recurrencia simple (puedes crecer esto luego)
    public bool IsRecurring { get; set; }

    [MaxLength(200)]
    public string? RecurrenceRule { get; set; } // estilo RRULE: "FREQ=WEEKLY;BYDAY=MO,WE"

    public DateTime? RecurrenceEndUtc { get; set; }

    // Excepciones de serie
    public Guid? ParentSeriesId { get; set; }
    public bool IsException { get; set; }

    // Estado
    public bool Active { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
}
