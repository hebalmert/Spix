using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Spix.AppFront.Shared;

public partial class InputDocument
{
    private string? FileName;
    private IBrowserFile? SelectedFile;
    private string? InternalPreviewUrl;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string AcceptExtensions { get; set; } = ".pdf,.doc,.docx,.xlsx,.xls";
    [Parameter] public string? PreviewUrl { get; set; }
    [Parameter] public long MaxAllowedSize { get; set; } = 10 * 1024 * 1024;
    [Parameter] public bool ShowPreview { get; set; } = true;
    [Parameter] public EventCallback<IBrowserFile> FileSelected { get; set; }
    [Parameter] public EventCallback<InputDocumentResult> DocumentSelected { get; set; }

    private string? CurrentPreviewUrl => InternalPreviewUrl ?? PreviewUrl;

    protected override void OnInitialized()
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            Label = "Documento";
        }
    }

    private async Task OnChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null || !IsValidExtension(file.Name))
            return;

        FileName = file.Name;
        SelectedFile = file;

        var buffer = new byte[file.Size];
        await file.OpenReadStream(MaxAllowedSize).ReadAsync(buffer);
        var base64 = Convert.ToBase64String(buffer);
        var dataUrl = $"data:{file.ContentType};base64,{base64}";

        InternalPreviewUrl = ShowPreview && IsPdf(file.Name) ? dataUrl : null;

        await FileSelected.InvokeAsync(file);
        await DocumentSelected.InvokeAsync(new InputDocumentResult
        {
            FileName = file.Name,
            ContentType = file.ContentType,
            Base64 = dataUrl,
            Size = file.Size
        });

        StateHasChanged();
    }

    private bool IsValidExtension(string fileName)
    {
        var allowed = AcceptExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(ext => ext.Trim().ToLowerInvariant());

        var fileExt = Path.GetExtension(fileName)?.ToLowerInvariant();
        return allowed.Contains(fileExt);
    }

    private static bool IsPdf(string fileName) =>
        string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase);
}

public class InputDocumentResult
{
    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public string Base64 { get; set; } = null!;

    public long Size { get; set; }
}
