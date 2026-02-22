using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Shared;

public partial class InputImageGeneral
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string? ImageBase64;
    private bool ShowCamera = true;
    private bool ShowPreview = false;
    private bool ShowImageUrl = true;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public EventCallback<string> ImageSelected { get; set; }

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(ImageUrl))
        {
            ShowPreview = true;
            ShowCamera = false;
        }
        else
        {
            ShowPreview = false;
            ShowCamera = false; // asegura que no se active por defecto
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await JS.InvokeVoidAsync("cameraInterop2.startCamera");
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
            var base64 = await JS.InvokeAsync<string>("cameraInterop2.takePhoto");
            if (!string.IsNullOrWhiteSpace(base64))
            {
                ImageBase64 = base64.Replace("data:image/jpeg;base64,", "");
                await ImageSelected.InvokeAsync(ImageBase64);
                ShowCamera = false;
                ShowPreview = true;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al capturar imagen: {ex.Message}");
        }
    }

    private void ToggleCamera()
    {
        ShowCamera = true;
        ShowPreview = false;
        ShowImageUrl = false;
    }

    private async Task LoadFromFile()
    {
        await JS.InvokeVoidAsync("cameraInterop2.loadFromFile", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public async Task OnImageLoaded(string base64Image)
    {
        if (!string.IsNullOrWhiteSpace(base64Image))
        {
            var base64Content = base64Image.Substring(base64Image.IndexOf(",") + 1);

            ImageBase64 = base64Content;
            await ImageSelected.InvokeAsync(ImageBase64);

            ShowCamera = false;
            ShowPreview = true;
            StateHasChanged();
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