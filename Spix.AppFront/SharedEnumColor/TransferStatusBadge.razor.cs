using Microsoft.AspNetCore.Components;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.SharedEnumColor;

public partial class TransferStatusBadge
{
    [Inject] private IEnumMultilLanguageService EnumMultilLanguageService { get; set; } = null!;

    [Parameter] public TransferType Value { get; set; }

    protected string Text => EnumMultilLanguageService.GetLocalizedName(Value);

    //Los mismos colores del sistema: ambar lo que sigue abierto, verde lo ya cerrado
    protected string Color => Value switch
    {
        TransferType.Pendiente => "#FD7E14",
        TransferType.Completado => "#198754",
        _ => "#6C757D"
    };
}
