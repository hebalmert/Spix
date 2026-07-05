using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestPic
{
    [Key]
    public Guid ServiceRequestPicId { get; set; }

    public Guid ServiceRequestId { get; set; }

    public DateTime? DateCreated { get; set; }

    public string? PhotoBefore1 { get; set; }

    public string? PhotoBefore2 { get; set; }

    public string? PhotoAfter1 { get; set; }

    public string? PhotoAfter2 { get; set; }

    [NotMapped]
    public string? ImageBefore1FullPath { get; set; }

    [NotMapped]
    public string? ImgBefore1Base64 { get; set; }

    [NotMapped]
    public string? ImageBefore2FullPath { get; set; }

    [NotMapped]
    public string? ImgBefore2Base64 { get; set; }

    [NotMapped]
    public string? ImageAfter1FullPath { get; set; }

    [NotMapped]
    public string? ImgAfter1Base64 { get; set; }

    [NotMapped]
    public string? ImageAfter2FullPath { get; set; }

    [NotMapped]
    public string? ImgAfter2Base64 { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }
}
