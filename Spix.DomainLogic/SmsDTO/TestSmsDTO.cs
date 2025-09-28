using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.DomainLogic.DTOs;

public class TestSmsDTO
{
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    public string? ToPhoneNumber { get; set; }

    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    public string? MessageSms { get; set; }
}