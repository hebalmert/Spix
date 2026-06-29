using Microsoft.Extensions.Localization;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppInfra.EnumMultilLanguage;

public class EnumMultilLanguageService : IEnumMultilLanguageService
{
    private readonly IStringLocalizer _localizer;

    public EnumMultilLanguageService(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public List<IntItemModel> GetEnumSelectList<TEnum>(string? firstOptionResourceKey = null, int firstOptionValue = 0)
        where TEnum : struct, Enum
    {
        var list = Enum.GetValues<TEnum>()
            .Select(value => new IntItemModel
            {
                Value = Convert.ToInt32(value),
                Name = GetLocalizedName(value)
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(firstOptionResourceKey))
        {
            list.Insert(0, new IntItemModel
            {
                Value = firstOptionValue,
                Name = GetLocalizedResource(firstOptionResourceKey)
            });
        }

        return list;
    }

    public string GetLocalizedName<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return GetLocalizedResource($"{typeof(TEnum).Name}_{value}");
    }

    private string GetLocalizedResource(string resourceKey)
    {
        var localizedValue = _localizer[resourceKey];

        return localizedValue.ResourceNotFound ? resourceKey : localizedValue.Value;
    }
}
