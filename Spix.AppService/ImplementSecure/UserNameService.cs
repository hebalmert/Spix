using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesSecure;
using Spix.DomainLogic.ModelUtility;
using System.Globalization;
using System.Text;

namespace Spix.Services.ImplementSecure;

//Nombres de usuario para login. El usuario es UNICO en toda la aplicacion (Identity),
//por eso se revisa aqui y no por corporacion.
public class UserNameService : IUserNameService
{
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;

    //Largo minimo del usuario: primero se completa con mas letras del apellido y, si aun no alcanza,
    //con numeros. Tambien es el minimo que se acepta al revisar uno escrito a mano.
    private const int MinLength = 8;

    private const int MaxSuggestions = 3;

    public UserNameService(IUserHelper userHelper, HttpErrorHandler httpErrorHandler)
    {
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<UserNameCheckDTO>> CheckAsync(string userName)
    {
        try
        {
            var clean = Normalize(userName);

            if (clean.Length < MinLength)
            {
                return Success(new UserNameCheckDTO { UserName = clean, Available = false });
            }

            var taken = await ExistsAsync(clean);

            var result = new UserNameCheckDTO
            {
                UserName = clean,
                Available = !taken
            };

            if (taken)
            {
                result.Suggestions = await BuildSuggestionsAsync(clean, string.Empty);
            }

            return Success(result);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<UserNameCheckDTO>(ex);
        }
    }

    public async Task<ActionResponse<UserNameCheckDTO>> SuggestAsync(string firstName, string lastName)
    {
        try
        {
            var name = Normalize(FirstWord(firstName));
            var surname = Normalize(lastName).Replace(" ", string.Empty);

            if (name.Length == 0 && surname.Length == 0)
            {
                return Success(new UserNameCheckDTO());
            }

            //Base: nombre + inicial del apellido. Si no llega al minimo, se toman mas letras del apellido.
            var baseName = name;
            var letters = 1;

            while (baseName.Length < MinLength && letters <= surname.Length)
            {
                baseName = name + surname[..letters];
                letters++;
            }

            if (baseName.Length < MinLength && surname.Length > 0)
            {
                baseName = name + surname;
            }

            if (baseName.Length == 0)
            {
                baseName = surname;
            }

            baseName = await EnsureMinLengthAsync(baseName);

            var free = await FirstFreeAsync(baseName, surname);

            return Success(new UserNameCheckDTO
            {
                UserName = free,
                Available = true,
                Suggestions = await BuildSuggestionsAsync(baseName, surname)
            });
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<UserNameCheckDTO>(ex);
        }
    }

    //Completa con numeros hasta llegar al minimo (ej. "anape" -> "anape123")
    private async Task<string> EnsureMinLengthAsync(string baseName)
    {
        if (baseName.Length >= MinLength || baseName.Length == 0)
        {
            return baseName;
        }

        var digits = MinLength - baseName.Length;
        var from = (int)Math.Pow(10, digits - 1);
        var to = from * 10;

        for (var i = from; i < to; i++)
        {
            var candidate = $"{baseName}{i}";
            if (!await ExistsAsync(candidate))
            {
                return candidate;
            }
        }

        return $"{baseName}{from}";
    }

    //Primera alternativa libre: base, base+apellido y luego base con numero
    private async Task<string> FirstFreeAsync(string baseName, string surname)
    {
        if (!await ExistsAsync(baseName))
        {
            return baseName;
        }

        var withSurname = baseName + surname;
        if (surname.Length > 0 && withSurname != baseName && !await ExistsAsync(withSurname))
        {
            return withSurname;
        }

        for (var i = 1; i <= 99; i++)
        {
            var candidate = $"{baseName}{i}";
            if (!await ExistsAsync(candidate))
            {
                return candidate;
            }
        }

        return $"{baseName}{DateTime.UtcNow:mmss}";
    }

    private async Task<List<string>> BuildSuggestionsAsync(string baseName, string surname)
    {
        var suggestions = new List<string>();

        var withSurname = baseName + surname;
        if (surname.Length > 0 && withSurname != baseName && !await ExistsAsync(withSurname))
        {
            suggestions.Add(withSurname);
        }

        for (var i = 1; i <= 99 && suggestions.Count < MaxSuggestions; i++)
        {
            var candidate = $"{baseName}{i}";
            if (!await ExistsAsync(candidate))
            {
                suggestions.Add(candidate);
            }
        }

        return suggestions;
    }

    private async Task<bool> ExistsAsync(string userName) =>
        await _userHelper.GetUserByUserNameAsync(userName) is not null;

    //Sin tildes, sin espacios, sin simbolos y en minusculas
    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var letter in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(letter) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(letter) || letter == '.' || letter == '_')
            {
                builder.Append(letter);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string FirstWord(string? value) =>
        value?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

    private static ActionResponse<UserNameCheckDTO> Success(UserNameCheckDTO result) =>
        new() { WasSuccess = true, Result = result };
}
