namespace Spix.DomainLogic.EnumTypes;

//Bitacora de la firma electronica: cada paso del proceso queda registrado.
//Es el audit trail que exige 21 CFR Part 11.10(e) y el equivalente al
//"Sent / Viewed / Signed" de DocuSign. Ver docs/Firma-Electronica-Part11.md
public enum SignatureEventType
{
    //La oficina le envio al cliente la invitacion a firmar
    RequestSent = 1,

    //El cliente abrio el documento para leerlo
    DocumentViewed = 2,

    //El cliente pidio el codigo de verificacion
    CodeSent = 3,

    //Escribio un codigo que no era
    CodeFailed = 4,

    //El codigo entro bien
    CodeValidated = 5,

    //Quedo firmado
    Signed = 6
}
