using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class FormServiceCategory
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter, EditorRequired] public ServiceCategory ServiceCategory { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    private string GetDisplayName<T>(Expression<Func<T>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            var property = memberExpression.Member as PropertyInfo;
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.Name!;
                }
            }
        }
        return "Texto no definido";
    }
}