using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppMaui.Services;
using Spix.DomainLogic.AppResponses;
using Spix.HttpService;

namespace Spix.AppMaui.ViewModels;

//El tecnico entra por el login de la web (v1). No se le hace uno aparte: no hay nada
//distinto que comprobar, y asi la contrasena se cambia en un solo sitio.
public partial class LoginViewModel : ObservableObject
{
    private const string LoginUrl = "api/v1/accounts/Login";

    private readonly IRepository _repository;
    private readonly SessionService _session;
    private readonly ApiResponseHandler _responseHandler;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public LoginViewModel(IRepository repository, SessionService session, ApiResponseHandler responseHandler)
    {
        _repository = repository;
        _session = session;
        _responseHandler = responseHandler;
    }

    [RelayCommand]
    private async Task EnterAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            return;
        }

        IsBusy = true;

        try
        {
            var modelo = new LoginDTO { UserName = UserName.Trim(), Password = Password };

            var response = await _repository.PostAsync<LoginDTO, TokenDTO>(LoginUrl, modelo);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var token = response.Response?.Token;
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            await _session.SetTokenAsync(token);

            Password = string.Empty;

            await Shell.Current.GoToAsync("//today");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
