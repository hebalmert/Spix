using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Spix.AppFront.Shared;

public partial class InputDocument
{
    private string? FileName;
    private IBrowserFile? SelectedFile;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string AcceptExtensions { get; set; } = ".pdf,.docx, .xlsx, .xls";
    [Parameter] public string? PreviewUrl { get; set; }
    [Parameter] public EventCallback<IBrowserFile> FileSelected { get; set; }

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

        await FileSelected.InvokeAsync(file);
        StateHasChanged();
    }

    private bool IsValidExtension(string fileName)
    {
        var allowed = AcceptExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(ext => ext.Trim().ToLowerInvariant());

        var fileExt = Path.GetExtension(fileName)?.ToLowerInvariant();
        return allowed.Contains(fileExt);
    }
}