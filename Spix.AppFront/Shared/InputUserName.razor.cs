using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Shared;

public partial class InputUserName : IDisposable
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;

    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    //Con estos dos se propone el usuario (nombre + inicial del apellido)
    [Parameter] public string? FirstName { get; set; }
    [Parameter] public string? LastName { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public string LabelWidth { get; set; } = "150px";
    [Parameter] public string? Placeholder { get; set; }

    //En edicion el usuario de login no se cambia
    [Parameter] public bool ReadOnly { get; set; }

    //true = al tener nombre y apellido propone el usuario solo (pantallas de creacion)
    [Parameter] public bool AutoSuggest { get; set; } = true;

    //Minimo de caracteres del usuario de login (igual que en el backend)
    [Parameter] public int MinLength { get; set; } = 8;
    [Parameter] public int DelayMs { get; set; } = 500;

    private const string BaseUrl = "api/v1/usernames";

    public enum UserNameStatus
    {
        Idle,
        TooShort,
        Checking,
        Available,
        Taken
    }

    private UserNameStatus Status { get; set; } = UserNameStatus.Idle;
    private List<string> Suggestions { get; set; } = new();
    private CancellationTokenSource? cts;
    private string lastChecked = string.Empty;
    private bool suggested;

    private string StatusCss => Status switch
    {
        UserNameStatus.Available => "is-available",
        UserNameStatus.Taken => "is-taken",
        _ => string.Empty
    };

    private string SuggestTitle => "Proponer usuario con el nombre y el apellido";
    private string AvailableText => "Usuario disponible";
    private string TakenText => "Este usuario ya existe";
    private string SuggestionsText => "Disponibles:";
    private string OtherOptionsText => "Otras opciones:";
    private string TooShortText => $"Escriba al menos {MinLength} caracteres";

    private bool suggestPending;

    protected override void OnParametersSet()
    {
        //Propone el usuario una sola vez, cuando ya hay nombre y apellido y el campo esta vacio.
        //Aqui solo se marca: la llamada al API se hace despues del render, nunca durante el.
        if (!AutoSuggest || ReadOnly || suggested)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(Value))
        {
            suggested = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            return;
        }

        suggested = true;
        suggestPending = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!suggestPending)
        {
            return;
        }

        suggestPending = false;
        await SuggestAsync();
    }

    private async Task OnInputAsync(ChangeEventArgs e)
    {
        var text = e.Value?.ToString() ?? string.Empty;
        await SetValueAsync(text);

        if (text.Trim().Length < MinLength)
        {
            Status = text.Trim().Length == 0 ? UserNameStatus.Idle : UserNameStatus.TooShort;
            Suggestions.Clear();
            return;
        }

        await CheckAsync(text.Trim(), immediate: false);
    }

    //Pide al backend un usuario propuesto con el nombre y el apellido
    private async Task SuggestAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
        {
            return;
        }

        Status = UserNameStatus.Checking;

        var url = $"{BaseUrl}/suggest?firstName={Uri.EscapeDataString(FirstName ?? string.Empty)}&lastName={Uri.EscapeDataString(LastName ?? string.Empty)}";
        var result = await CallAsync(url);

        if (result is null || string.IsNullOrWhiteSpace(result.UserName))
        {
            Status = UserNameStatus.Idle;
            StateHasChanged();
            return;
        }

        Suggestions = result.Suggestions ?? new List<string>();
        lastChecked = result.UserName;
        Status = UserNameStatus.Available;

        await SetValueAsync(result.UserName);
        StateHasChanged();
    }

    private async Task UseSuggestionAsync(string option)
    {
        await SetValueAsync(option);
        await CheckAsync(option, immediate: true);
    }

    //Revisa contra Identity; con pausa para no consultar en cada tecla
    private async Task CheckAsync(string userName, bool immediate)
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

        if (token.IsCancellationRequested)
        {
            return;
        }

        Status = UserNameStatus.Checking;
        StateHasChanged();

        var result = await CallAsync($"{BaseUrl}/check?userName={Uri.EscapeDataString(userName)}");

        if (token.IsCancellationRequested)
        {
            return;
        }

        if (result is null)
        {
            Status = UserNameStatus.Idle;
            StateHasChanged();
            return;
        }

        lastChecked = result.UserName;
        Suggestions = result.Suggestions ?? new List<string>();
        Status = result.Available ? UserNameStatus.Available : UserNameStatus.Taken;

        StateHasChanged();
    }

    //El repositorio revienta si la respuesta viene vacia, por eso se atrapa todo aqui:
    //si el API falla, el campo sigue sirviendo escribiendo a mano y el backend valida al guardar.
    private async Task<UserNameCheckDTO?> CallAsync(string url)
    {
        try
        {
            var responseHttp = await _repository.GetAsync<UserNameCheckDTO>(url);
            return responseHttp.Error ? null : responseHttp.Response;
        }
        catch
        {
            return null;
        }
    }

    private async Task SetValueAsync(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
