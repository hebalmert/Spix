using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;
using Spix.AppFront.AuthenticationProviders;
using System.Net;
using Spix.HttpService;

namespace Spix.AppFront.Helper;

public class HttpResponseHandler
{
    private readonly ILoginService _loginService;
    private readonly NavigationManager _navigationManager;
    private readonly SweetAlertService _sweetAlert;
    private readonly IStringLocalizer<Resource> Localizer;

    public HttpResponseHandler(
        ILoginService loginService,
        NavigationManager navigationManager,
        SweetAlertService sweetAlert, IStringLocalizer<Resource> localizer)
    {
        _loginService = loginService;
        _navigationManager = navigationManager;
        _sweetAlert = sweetAlert;
        Localizer = localizer;
    }

    public async Task<bool> HandleErrorAsync<T>(HttpResponseWrapper<T> responseHttp)
    {
        if (responseHttp.HttpResponseMessage == null)
            return false;

        var statusCode = responseHttp.HttpResponseMessage.StatusCode;
        var errorMessage = await responseHttp.GetErrorMessageAsync();

        string title, message;
        SweetAlertIcon icon;

        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
                if (errorMessage?.Contains("Token Expirado", StringComparison.OrdinalIgnoreCase) == true ||
                    errorMessage?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true)
                {
                    title = Localizer[nameof(Resource.HttpCode_UnauthorizedTokenTitle)];
                    message = Localizer[nameof(Resource.HttpCode_UnauthorizedTokenMsg)];
                    icon = SweetAlertIcon.Warning;
                }
                else
                {
                    title = Localizer[nameof(Resource.HttpCode_UnauthorizedDeniedTitle)];
                    message = Localizer[nameof(Resource.HttpCode_UnauthorizedDeniedMsg)];
                    icon = SweetAlertIcon.Error;
                }

                await _loginService.LogoutAsync();
                _navigationManager.NavigateTo("/");
                break;

            case HttpStatusCode.Forbidden:
                title = Localizer[nameof(Resource.HttpCode_ForbiddenTile)];
                message = Localizer[nameof(Resource.HttpCode_ForbiddenMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            case HttpStatusCode.NotFound:
                title = Localizer[nameof(Resource.HttpCode_NotFoundTitle)];
                message = title = Localizer[nameof(Resource.HttpCode_NotFoundMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            case HttpStatusCode.InternalServerError:
                title = title = Localizer[nameof(Resource.HttpCode_ServerErrorTitle)];
                message = title = Localizer[nameof(Resource.HttpCode_ServerErrorMsg)];
                icon = SweetAlertIcon.Error;
                break;

            case HttpStatusCode.BadRequest:
                bool isLoginError =
                    errorMessage?.Contains("Invalid credentials", StringComparison.OrdinalIgnoreCase) == true ||
                    errorMessage?.Contains("username", StringComparison.OrdinalIgnoreCase) == true ||
                    errorMessage?.Contains("password", StringComparison.OrdinalIgnoreCase) == true;

                if (isLoginError)
                {
                    title = title = Localizer[nameof(Resource.HttpCode_LoginTitle)];
                    message = Localizer[nameof(Resource.HttpCode_LoginMsg)];
                    icon = SweetAlertIcon.Warning;
                }
                else
                {
                    title = Localizer[nameof(Resource.HttpCode_BadRequestTitle)];
                    message = errorMessage ?? Localizer[nameof(Resource.HttpCode_BadRequestMsg)];
                    icon = SweetAlertIcon.Warning;
                }
                break;

            case HttpStatusCode.GatewayTimeout:
                title = Localizer[nameof(Resource.httpCode_GeteWayTimeTitle)];
                message = Localizer[nameof(Resource.httpCode_GeteWayTimeMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            case HttpStatusCode.ServiceUnavailable:
                title = Localizer[nameof(Resource.httpCode_ServiceTitle)];
                message = Localizer[nameof(Resource.httpCode_ServiceMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            case HttpStatusCode.BadGateway:
                title = Localizer[nameof(Resource.httpCode_BadGatewayTitle)];
                message = Localizer[nameof(Resource.httpCode_BadGatewayMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            case HttpStatusCode.RequestTimeout:
                title = Localizer[nameof(Resource.httpCode_TimeoutTitle)];
                message = Localizer[nameof(Resource.httpCode_TimeoutMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            case HttpStatusCode.UnprocessableEntity:
                title = Localizer[nameof(Resource.httpCode_UnprocessTitle)];
                message = Localizer[nameof(Resource.httpCode_UnprocessMsg)];
                icon = SweetAlertIcon.Warning;
                break;

            default:
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    title = "Error";
                    message = errorMessage;
                    icon = SweetAlertIcon.Error;
                    break;
                }
                return false;
        }

        await _sweetAlert.FireAsync(title, message, icon);
        return true;
    }
}
