using Microsoft.AspNetCore.Components;
using Spix.AppFront.Shared;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class FormContractDocumentTemplate
{
    [Parameter, EditorRequired] public ContractDocumentTemplate Template { get; set; } = null!;
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public EventCallback ReturnAction { get; set; }

    private void DocumentSelected(InputDocumentResult document)
    {
        Template.FileBase64 = document.Base64;
        Template.OriginalFileName = document.FileName;
    }

    private void DocumentTypeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            Template.DocumentType = (ContractDocumentType)value;
    }
}
