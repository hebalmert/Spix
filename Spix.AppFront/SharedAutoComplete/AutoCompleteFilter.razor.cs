using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.SharedAutoComplete;

//Buscador que filtra un listado. No selecciona un registro (para eso esta AutoCompleteGuidSelect):
//solo avisa el texto al padre para que recargue su lista.
public partial class AutoCompleteFilter : IDisposable
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public EventCallback<string> ApplyFilter { get; set; }

    [Parameter] public string Placeholder { get; set; } = "Escriba para buscar...";

    //Minimo de caracteres para consultar
    [Parameter] public int MinLength { get; set; } = 3;

    //Pausa despues de la ultima tecla antes de consultar (modo al escribir)
    [Parameter] public int DelayMs { get; set; } = 500;

    //true = busca SOLO con Enter o con la lupa. Para tablas grandes, donde cada consulta pesa.
    [Parameter] public bool SearchOnEnter { get; set; }

    [Parameter] public string EnterHint { get; set; } = "Escriba y presione Enter para buscar";

    private string SearchText = string.Empty;
    private string lastSent = string.Empty;
    private CancellationTokenSource? cts;

    private async Task OnSearchChangedAsync()
    {
        var text = SearchText?.Trim() ?? string.Empty;

        //Con Enter no se busca al escribir; solo se avisa cuando el campo queda vacio,
        //para que el padre vuelva a mostrar la lista completa.
        if (SearchOnEnter)
        {
            if (text.Length == 0 && lastSent.Length > 0)
            {
                await SendAsync(string.Empty, immediate: true);
            }

            return;
        }

        //Menos del minimo: no se consulta (el vacio si pasa, para limpiar el filtro)
        if (text.Length > 0 && text.Length < MinLength)
        {
            return;
        }

        await SendAsync(text);
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SearchNowAsync();
        }
    }

    private async Task SearchNowAsync()
    {
        var text = SearchText?.Trim() ?? string.Empty;

        if (text.Length > 0 && text.Length < MinLength)
        {
            return;
        }

        await SendAsync(text, immediate: true);
    }

    private async Task ClearAsync()
    {
        SearchText = string.Empty;
        await SendAsync(string.Empty, immediate: true);
    }

    private async Task SendAsync(string text, bool immediate = false)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        if (!immediate)
        {
            try
            {
                await Task.Delay(DelayMs, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        if (token.IsCancellationRequested || text == lastSent)
        {
            return;
        }

        lastSent = text;
        await ApplyFilter.InvokeAsync(text);
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
