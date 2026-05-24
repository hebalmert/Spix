using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

public class ContractIDPic
{
    [Key]
    public Guid ContractIDPicId { get; set; }

    public Guid ContractClientId { get; set; }

    public DateTime? DateCreado { get; set; }

    [Display(Name = nameof(Resource.Photo), ResourceType = typeof(Resource))]
    public string? PhotoIDFront { get; set; }

    [Display(Name = nameof(Resource.Photo), ResourceType = typeof(Resource))]
    public string? PhotoIDBack { get; set; }


    [NotMapped]
    public string? ImageFrontFullPath { get; set; }

    [NotMapped]
    public string? ImgFrontBase64 { get; set; }


    [NotMapped]
    public string? ImageBackFullPath { get; set; }

    [NotMapped]
    public string? ImgBackBase64 { get; set; }


    //Cual fue el usuario que Creo el Registro
    [Display(Name = "Quien Creo el QcGeneral")]  //El usuario que creo el QC su nombre en base al logueo del usuario.
    public string? UsuarioOwner { get; set; }

    [Display(Name = "Quien Creo el QcGeneral")]  //El UserId del usuario que lo creo para ubicarle sus credenciales.
    public Guid? UserId { get; set; }
    //Fin quien creo el registro

    public int CorporationId { get; set; }
    public Corporation? Corporation { get; set; }
    public ContractClient? ContractClient { get; set; }
}
