using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;

namespace Spix.AppFront.Pages.EntitiesContratos.MySignaturePage;

public partial class CertificateMyDocument
{
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter, EditorRequired] public string Code { get; set; } = string.Empty;

    [Parameter] public string? Title { get; set; }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
