using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesSchedule;

//Una foto de la visita. Antes eran cuatro columnas fijas en ServiceRequestPic, asi que
//al borrar la segunda quedaba un hueco y no se podian repartir distinto. Ahora cada foto
//es un registro: se agregan y se borran de a una, hasta el tope que fija el servicio.
public class ServiceRequestPhoto
{
    [Key]
    public Guid ServiceRequestPhotoId { get; set; }

    [Required]
    public Guid ServiceRequestId { get; set; }

    //Como quedo guardada en el blob
    [MaxLength(200)]
    public string? Photo { get; set; }

    //Antes o despues de la reparacion: es lo que sostiene el cierre guiado
    public ServicePhotoType PhotoType { get; set; }

    public DateTime DateCreated { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    public Guid? UserId { get; set; }

    public int CorporationId { get; set; }

    //Lo que viaja a la pantalla, no a la base
    [NotMapped]
    public string? ImageFullPath { get; set; }

    [NotMapped]
    public string? ImgBase64 { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public Corporation? Corporation { get; set; }
}
