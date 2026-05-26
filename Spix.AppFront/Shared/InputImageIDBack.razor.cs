using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Shared;

public partial class InputImageIDBack
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string? ImageBase64;
    private bool ShowCamera = false;
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
            ShowCamera = false; // cámara apagada por defecto
        }
    }

    private async Task CaptureImage()
    {
        try
        {
            var base64 = await JS.InvokeAsync<string>("camaraIdPicBack.takePhoto");
            if (!string.IsNullOrWhiteSpace(base64))
            {
                ImageBase64 = base64.Replace("data:image/jpeg;base64,", "");
                await ImageSelected.InvokeAsync(ImageBase64);
                await JS.InvokeVoidAsync("camaraIdPicBack.stopCamera");
                ShowCamera = false;
                ShowCamera = false;
                ShowPreview = true;
                ShowImageUrl = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al capturar imagen: {ex.Message}");
        }
    }

    private async Task ToggleCamera()
    {
        ShowCamera = true;
        ShowPreview = false;
        ShowImageUrl = false;

        await JS.InvokeVoidAsync("camaraIdPicBack.startCamera");
    }

    private async Task CloseCamera()
    {
        ShowCamera = false;
        ShowPreview = false;

        await JS.InvokeVoidAsync("camaraIdPicBack.stopCamera");
        if (!string.IsNullOrWhiteSpace(ImageUrl) && string.IsNullOrWhiteSpace(ImageBase64))
        {
            ShowImageUrl = true;
        }
        StateHasChanged();
    }

    private async Task LoadFromFile()
    {
        await JS.InvokeVoidAsync("camaraIdPicBack.loadFromFile", DotNetObjectReference.Create(this));
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
            ShowImageUrl = false;
            StateHasChanged();
        }
    }
}