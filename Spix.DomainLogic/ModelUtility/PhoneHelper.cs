namespace Spix.DomainLogic.ModelUtility;

// Arma el telefono a partir de sus tres partes.
//
// El numero se guarda partido para poder enviarle mensajes: WhatsApp y SMS piden el
// formato internacional, y de un campo suelto como "300 367 5815" no hay forma segura
// de deducir el pais.
//
// Las tres partes se escriben a mano. En una misma empresa hay clientes de paises
// distintos (+57 y +58 juntos), asi que el indicativo no se deduce de ningun lado.
public static class PhoneHelper
{
    // El numero listo para enviar: +573003675815
    //
    // Sin signos ni espacios, que es como lo piden las pasarelas. Devuelve vacio si
    // falta el numero, para que quien envie pueda saltarse ese contacto sin reventar.
    public static string Internacional(string? codeCountry, string? codeNumber, string? phoneNumber)
    {
        var numero = SoloDigitos(phoneNumber);

        if (string.IsNullOrEmpty(numero))
        {
            return string.Empty;
        }

        var pais = SoloDigitos(codeCountry);

        if (string.IsNullOrEmpty(pais))
        {
            return string.Empty;
        }

        return "+" + pais + SoloDigitos(codeNumber) + numero;
    }

    // El numero para LEER en pantalla: +57 300 3675815
    public static string Visible(string? codeCountry, string? codeNumber, string? phoneNumber)
    {
        var numero = (phoneNumber ?? string.Empty).Trim();

        if (numero.Length == 0)
        {
            return string.Empty;
        }

        var pais = (codeCountry ?? string.Empty).Trim();
        var area = (codeNumber ?? string.Empty).Trim();

        //Las partes que falten simplemente no se escriben
        var partes = new List<string>();

        if (pais.Length > 0)
        {
            partes.Add(pais.StartsWith('+') ? pais : "+" + pais);
        }

        if (area.Length > 0)
        {
            partes.Add(area);
        }

        partes.Add(numero);

        return string.Join(" ", partes);
    }

    // Quita el mas, los espacios, los guiones y los parentesis
    private static string SoloDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        return new string(valor.Where(char.IsDigit).ToArray());
    }
}
