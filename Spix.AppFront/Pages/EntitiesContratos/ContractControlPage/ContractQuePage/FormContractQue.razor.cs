using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractQuePage;

public partial class FormContractQue
{
    [Parameter, EditorRequired] public ContractQue ContractQue { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }
}
