using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesSchedule;

public class ScheduleItem
{
    [Key]
    public Guid ScheduleItemId { get; set; }

    // Titulo y descripcion
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

    // Tecnico al que se le asigna la tarea
    [Required]
    public Guid TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    //Cual fue el usuario que creo el registro
    [Display(Name = "Quien Creo el QcGeneral")]
    public string? UsuarioOwner { get; set; }

    [Display(Name = "Quien Creo el QcGeneral")]
    public Guid? UserId { get; set; }

    // Recurrencia simple
    public bool IsRecurring { get; set; }

    [MaxLength(200)]
    public string? RecurrenceRule { get; set; }

    public DateTime? RecurrenceEndUtc { get; set; }

    // Excepciones de serie
    public Guid? ParentSeriesId { get; set; }
    public bool IsException { get; set; }

    // Estado
    public bool Active { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    //Status
    public ScheduleStatus? ScheduleStatus { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
}
