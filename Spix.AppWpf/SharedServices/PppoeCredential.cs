using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Spix.AppWpf.SharedServices;

//El usuario PPPoE, con la MISMA receta del Blazor (FormContractPppoe): el boton propone
//apellido + numero de contrato + 3 al azar, y la regla exige minimo 6 con letras y numeros.
//Va recreado aca, no compartido: el escritorio no referencia al front.
//Si la regla cambia en el Blazor o en la entidad, cambia tambien aqui.
public static class PppoeCredential
{
    //Sin las que se confunden al dictar por telefono: 0 y O, 1 y l
    private const string Alfabeto = "23456789abcdefghjkmnpqrstuvwxyz";

    //La misma expresion del atributo de ContractPppoe.Usuario
    private static readonly Regex Regla =
        new(@"^(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9._-]{6,50}$", RegexOptions.Compiled);

    public const string ReglaTexto =
        "El usuario PPPoE debe tener al menos 6 caracteres, con letras y numeros. " +
        "Solo se permiten letras, numeros, punto, guion y guion bajo.";

    public static bool EsUsuarioValido(string? usuario)
    {
        return !string.IsNullOrWhiteSpace(usuario) && Regla.IsMatch(usuario);
    }

    //Ej: contrato 1 de Merchan -> "mer1k7q". Se reconoce de quien es, lleva el numero del
    //contrato y no se adivina.
    public static string ProponerUsuario(string? apellido, string? controlContrato)
    {
        var prefijo = SoloLetras(apellido);
        var numero = SoloNumeros(controlContrato);

        //Sin apellido usable queda solo numero + azar; el prefijo fijo mantiene la letra que
        //exige la regla y el largo minimo.
        if (prefijo.Length == 0)
        {
            prefijo = "cli";
        }

        if (numero.Length == 0)
        {
            numero = "0";
        }

        return prefijo + numero + AlAzar(3);
    }

    //Una clave corta y al azar, igual que en el Blazor
    public static string GenerarClave()
    {
        return Guid.NewGuid().ToString("N")[..10];
    }

    //Las primeras 3 letras del apellido, en minuscula y sin tildes ni ñ: el usuario viaja por
    //el equipo, no es sitio para caracteres raros.
    private static string SoloLetras(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var limpio = new StringBuilder();
        foreach (var letra in texto.Normalize(NormalizationForm.FormD))
        {
            var minuscula = char.ToLowerInvariant(letra);
            if (minuscula >= 'a' && minuscula <= 'z')
            {
                limpio.Append(minuscula);
            }

            if (limpio.Length == 3)
            {
                break;
            }
        }

        return limpio.ToString();
    }

    private static string SoloNumeros(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var limpio = new StringBuilder();
        foreach (var caracter in texto)
        {
            if (char.IsDigit(caracter))
            {
                limpio.Append(caracter);
            }
        }

        return limpio.ToString();
    }

    private static string AlAzar(int largo)
    {
        var texto = new StringBuilder(largo);
        for (var i = 0; i < largo; i++)
        {
            texto.Append(Alfabeto[Random.Shared.Next(Alfabeto.Length)]);
        }

        return texto.ToString();
    }
}
