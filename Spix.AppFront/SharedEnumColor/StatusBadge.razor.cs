using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppInfra.Extensions;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.SharedEnumColor;

public partial class StatusBadge
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter] public ContractState Value { get; set; }

    protected string Text => Value.ToLocalizedString(Localizer);

    protected string Color => Value switch
    {
        ContractState.Draft => "#0D6EFD", // Azul
        ContractState.PendingApproval => "#FD7E14", // Naranja
        ContractState.Active => "#198754", // Verde
        ContractState.Exempt => "#20C997", // Verde agua
        ContractState.Suspended => "#DC3545", // Rojo
        ContractState.Cancelled => "#B02A37", // Rojo oscuro
        ContractState.Terminated => "#6F42C1", // Púrpura
        _ => "#6C757D"
    };
}