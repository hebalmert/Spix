using Microsoft.AspNetCore.Components;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.SharedEnumColor;

public partial class PurchaseStatusBadge
{
    [Inject] private IEnumMultilLanguageService EnumMultilLanguageService { get; set; } = null!;

    [Parameter] public PurchaseStatus Value { get; set; }

    protected string Text => EnumMultilLanguageService.GetLocalizedName(Value);

    protected string Color => Value switch
    {
        PurchaseStatus.Pendiente => "#FD7E14",
        PurchaseStatus.Completado => "#198754",
        _ => "#6C757D"
    };
}
