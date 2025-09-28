using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Spix.AppFront.Shared;

public partial class InputDocument
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string AcceptExtensions { get; set; } = ".xlsx";
    [Parameter] public EventCallback<IBrowserFile> FileSelected { get; set; }

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private string? FileName;
    private IBrowserFile? SelectedFile;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (string.IsNullOrWhiteSpace(Label))
        {
            Label = "Excel Document";
        }
    }

    private async Task OnChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null || !IsValidExtension(file.Name) || file.Size > MaxFileSize)
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