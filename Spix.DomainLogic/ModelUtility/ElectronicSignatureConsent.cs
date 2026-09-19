namespace Spix.DomainLogic.ModelUtility;

//Aviso de firma electronica que el cliente debe aceptar antes de firmar.
//El texto vive aqui, no en el navegador: asi se guarda con el documento la version exacta
//que acepto el firmante y su huella. Si el texto cambia, hay que subir la version.
public static class ElectronicSignatureConsent
{
    public const string Version = "1.0";

    public const string Text =
        "Acepto usar medios electronicos para leer y firmar este documento, y reconozco que la firma " +
        "electronica que aplique tiene la misma validez y fuerza obligatoria que una firma manuscrita. " +
        "Entiendo que mi identidad se verifica con la clave de mi cuenta y con un codigo de un solo uso " +
        "enviado al correo registrado, y que el sistema deja constancia de la fecha y hora, la direccion IP " +
        "y el navegador desde donde firmo. Declaro ademas haber leido el documento completo y estar conforme " +
        "con su contenido.";
}
