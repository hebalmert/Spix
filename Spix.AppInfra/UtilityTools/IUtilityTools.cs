namespace Spix.AppInfra.UtilityTools;

public interface IUtilityTools
{
    // Version flexible: el usuario define los caracteres
    string GeneratePass(int longitud, string caracteres);

    // Version estandar: usa caracteres seguros internos
    string GeneratePass(int longitud);

}