using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class ViewContractPdf
{
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    [Parameter] public string? PdfUrl { get; set; }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
