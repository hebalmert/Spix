using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Spix.AppFront.Shared;

public partial class InputImage
{
    private string? ImageBase64;
    private string? FileName;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public EventCallback<string> ImageSelected { get; set; }

    protected override void OnInitialized()
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            Label = "Imagen";
        }
    }

    private async Task OnChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null || file.Size == 0)
            return;

        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            return;

        FileName = file.Name;

        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
            var arrBytes = new byte[file.Size];
            await stream.ReadAsync(arrBytes);
            ImageBase64 = Convert.ToBase64String(arrBytes);
            ImageUrl = null;
            await ImageSelected.InvokeAsync(ImageBase64);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error leyendo imagen: {ex.Message}");
            ImageBase64 = null;
            FileName = null;
        }
    }

    private string GetImageSource()
    {
        if (!string.IsNullOrWhiteSpace(ImageUrl))
            return ImageUrl; // SAS URL ya lista para mostrar

        if (!string.IsNullOrWhiteSpace(ImageBase64))
            return $"data:image/jpeg;base64,{ImageBase64}";

        return ImageUrl ?? string.Empty; // ya viene como data URI, listo para mostrar
    }
}