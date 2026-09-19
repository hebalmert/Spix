namespace Spix.DomainLogic.EnumTypes;

//Como se verifico la identidad del firmante. Se guarda con el documento firmado (ver docs/Firma-Electronica-Part11.md)
public enum SignatureMethod
{
    //Firmo desde su cuenta, con codigo de un solo uso enviado a su correo
    PortalVerified = 1,

    //Firmo en el dispositivo del asesor, pero igual valido el codigo de su correo
    OfficeVerified = 2,

    //Firmo en oficina sin poder recibir el codigo: queda el asesor como testigo
    OfficeAssisted = 3
}
