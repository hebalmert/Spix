using Microsoft.AspNetCore.Components;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.SharedEnumColor;

public partial class StatusBadge
{
    [Inject] private IEnumMultilLanguageService EnumMultilLanguageService { get; set; } = null!;

    [Parameter] public ContractState Value { get; set; }

    protected string Text => EnumMultilLanguageService.GetLocalizedName(Value);

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
