using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Regix.AppFront.Shared;

public partial class InputImageWithFrame
{
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string? ImageBase64;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public EventCallback<string> ImageSelected { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await JS.InvokeVoidAsync("cameraInterop.startCamera");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar cámara: {ex.Message}");
            }
        }
    }

    private async Task CaptureImage()
    {
        try
        {
            var base64 = await JS.InvokeAsync<string>("cameraInterop.takePhoto");
            if (!string.IsNullOrWhiteSpace(base64))
            {
                ImageBase64 = base64.Replace("data:image/jpeg;base64,", "");
                await ImageSelected.InvokeAsync(ImageBase64);
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al capturar imagen: {ex.Message}");
        }
    }

    private string GetImageSource()
    {
        if (!string.IsNullOrWhiteSpace(ImageUrl))
            return ImageUrl;

        if (!string.IsNullOrWhiteSpace(ImageBase64))
            return $"data:image/jpeg;base64,{ImageBase64}";

        return string.Empty;
    }
}