using Microsoft.AspNetCore.Components;

namespace Spix.AppFront.Shared;

// Aloja el modal que abre ModalService. Replicado de Rentx.
public partial class ModalHost
{
    [Parameter] public Type? ComponentType { get; set; }

    [Parameter] public IDictionary<string, object>? Parameters { get; set; }
}
