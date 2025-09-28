using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.DomainLogic.DTOs;

public class SmsBulkDTO
{
    //Para subir el archivo de Excel
    public string? ImgBase64 { get; set; }

    //Para tener el nombre original del archivo de Excel
    public string? FileNameOriginal { get; set; }

    //Mensaje que se enviara a la lista de excel
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    public string? MessageSms { get; set; }
}