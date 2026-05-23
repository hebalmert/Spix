using Microsoft.Extensions.Localization;

namespace Spix.AppInfra.Extensions;

public static class EnumExtensions
{
    public static string ToLocalizedString<TEnum>(this TEnum value, IStringLocalizer localizer)
        where TEnum : Enum
    {
        return localizer[$"{typeof(TEnum).Name}_{value}"];
    }
}
