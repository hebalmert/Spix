using Microsoft.AspNetCore.Components;
using Spix.AppFront.Shared;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class FormContractDocumentTemplate
{
    [Parameter, EditorRequired] public ContractDocumentTemplate Template { get; set; } = null!;
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool ShowFields { get; set; }
    [Parameter] public List<ContractDocumentTemplateField> Fields { get; set; } = new();
    [Parameter] public ContractDocumentTemplateField NewField { get; set; } = new();
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public EventCallback ReturnAction { get; set; }
    [Parameter] public EventCallback<ContractDocumentTemplateField> AddFieldAction { get; set; }
    [Parameter] public EventCallback<ContractDocumentTemplateField> DeleteFieldAction { get; set; }

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

    private void FieldTypeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            NewField.FieldType = (ContractDocumentFieldType)value;
    }

    private async Task AddField()
    {
        await AddFieldAction.InvokeAsync(NewField);
    }

    private async Task DeleteField(ContractDocumentTemplateField field)
    {
        await DeleteFieldAction.InvokeAsync(field);
    }

    private static string GetFieldName(ContractDocumentFieldType fieldType) =>
        fieldType switch
        {
            ContractDocumentFieldType.FullName => "Nombre",
            ContractDocumentFieldType.Document => "Documento",
            ContractDocumentFieldType.Phone => "Telefono",
            ContractDocumentFieldType.Date => "Fecha",
            ContractDocumentFieldType.Signature => "Firma",
            _ => fieldType.ToString()
        };
}
