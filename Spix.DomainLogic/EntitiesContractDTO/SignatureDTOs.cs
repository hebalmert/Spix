using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesContractDTO;

//Documento que el cliente ve en su portal
public class MySignatureDocumentDTO
{
    public Guid ContractClientId { get; set; }

    public long ContractNumber { get; set; }

    //Con que se distingue un contrato de otro del mismo cliente
    public DateTime ContractDate { get; set; }

    public string? ContractAddress { get; set; }

    public string? ZoneName { get; set; }

    public string? PlanName { get; set; }

    public ContractDocumentType DocumentType { get; set; }

    public bool Signed { get; set; }

    public DateTime? DateSigned { get; set; }

    //Identificador del certificado: con el se abre el historial de la firma
    public string? VerificationCode { get; set; }
}

//Respuesta al pedir el codigo de verificacion
public class SignatureCodeDTO
{
    //Correo enmascarado, para confirmarle al cliente a donde se envio
    public string MaskedEmail { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}

//Lo que envia el cliente para firmar
public class SignDocumentRequestDTO
{
    public Guid ContractClientId { get; set; }

    public ContractDocumentType DocumentType { get; set; }

    public string Code { get; set; } = string.Empty;

    public string SignatureBase64 { get; set; } = string.Empty;

    //Casilla "He leido el documento y estoy conforme"
    public bool TermsAccepted { get; set; }
}

//Enlace firmado del PDF. Va en un objeto y no como texto suelto: una cadena pelada
//viaja como text/plain y el cliente no la puede deserializar.
public class SignatureLinkDTO
{
    public string Url { get; set; } = string.Empty;
}

//Aviso de firma electronica que se muestra antes de firmar
public class SignatureTermsDTO
{
    public string Version { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}

//Un paso de la bitacora, tal como se muestra en la verificacion publica
public class SignatureEventDTO
{
    public SignatureEventType EventType { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Detail { get; set; }

    //Origen del paso. Solo viaja si quien consulta es el firmante o alguien de la corporacion.
    public string? SourceIp { get; set; }

    public string? UserAgent { get; set; }
}

//Lo que ve cualquiera que entre a verificar un documento firmado.
//NO lleva datos personales completos: el nombre y el correo van enmascarados.
public class SignatureVerificationDTO
{
    public bool Found { get; set; }

    public string VerificationCode { get; set; } = string.Empty;

    public string? CorporationName { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    public long ContractNumber { get; set; }

    public string SignerName { get; set; } = string.Empty;

    public string SignerEmail { get; set; } = string.Empty;

    public DateTime? SignedAt { get; set; }

    public string Method { get; set; } = string.Empty;

    //Huella del documento firmado (la impresa) y del archivo completo (la que puede
    //comprobar quien descargue el PDF)
    public string? DocumentHash { get; set; }

    public string? FileHash { get; set; }

    public string? ConsentVersion { get; set; }

    //Desde donde se firmo. Solo viaja si quien consulta es el firmante o alguien de la corporacion.
    public string? SignerIp { get; set; }

    public string? SignerUserAgent { get; set; }

    public List<SignatureEventDTO> Events { get; set; } = new();
}
