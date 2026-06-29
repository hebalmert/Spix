using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppInfra.EnumMultilLanguage;

public interface IEnumMultilLanguageService
{
    List<IntItemModel> GetEnumSelectList<TEnum>(string? firstOptionResourceKey = null, int firstOptionValue = 0)
        where TEnum : struct, Enum;

    string GetLocalizedName<TEnum>(TEnum value)
        where TEnum : struct, Enum;
}
