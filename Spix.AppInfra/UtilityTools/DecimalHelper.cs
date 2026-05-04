namespace Spix.AppInfra.UtilityTools;

public static class DecimalHelper
{
    public static decimal FormatDecimal(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
