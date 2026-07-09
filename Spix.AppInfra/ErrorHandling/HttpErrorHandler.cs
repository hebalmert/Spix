using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppInfra.ErrorHandling;

public class HttpErrorHandler
{
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<HttpErrorHandler> _logger;

    public HttpErrorHandler(IStringLocalizer localizer, ILogger<HttpErrorHandler> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    public Task<ActionResponse<T>> HandleErrorAsync<T>(Exception exception)
    {
        string errorMessage = _localizer[nameof(Resource.Generic_UnexpectedError)];

        if (exception is null)
        {
            errorMessage = _localizer[nameof(Resource.Generic_NullException)];

            _logger.LogError("Null exception received in HttpErrorHandler");

            return Task.FromResult(new ActionResponse<T>
            {
                WasSuccess = false,
                Message = errorMessage,
                Result = default
            });
        }

        _logger.LogError(
            exception,
            "Error capturado en HttpErrorHandler. Tipo: {ExceptionType}, Mensaje: {ExceptionMessage}",
            exception.GetType().Name,
            exception.Message
        );

        if (exception is DbUpdateConcurrencyException)
        {
            errorMessage = _localizer[nameof(Resource.Db_Concurrency)];
        }
        else if (exception is UnauthorizedAccessException)
        {
            errorMessage = _localizer[nameof(Resource.Generic_AccessDenied)];
        }
        else if (exception is TimeoutException)
        {
            errorMessage = _localizer["Generic_Timeout"];
        }
        else if (exception is TaskCanceledException)
        {
            errorMessage = _localizer["Generic_Timeout"];
        }
        else if (exception is OperationCanceledException)
        {
            errorMessage = _localizer["Generic_RequestCanceled"];
        }
        else if (exception is ArgumentNullException)
        {
            errorMessage = _localizer[nameof(Resource.Generic_NullException)];
        }
        else if (exception is ArgumentException)
        {
            errorMessage = _localizer[nameof(Resource.Generic_InvalidModel)];
        }
        else if (exception is InvalidOperationException)
        {
            errorMessage = _localizer["Generic_InvalidOperation"];
        }
        else if (exception is KeyNotFoundException)
        {
            errorMessage = _localizer[nameof(Resource.Generic_RegisterNotFound)];
        }
        else if (TryGetSqlErrorMessage(exception, out string? sqlErrorMessage))
        {
            errorMessage = sqlErrorMessage!;
        }
        else if (exception is HttpRequestException httpEx)
        {
            errorMessage = $"{_localizer[nameof(Resource.Generic_Http_BadRequest)]}: {httpEx.Message}";
        }
        else if (exception is DbUpdateException dbEx)
        {
            if (dbEx.InnerException != null && TryGetSqlErrorMessage(dbEx.InnerException, out string? innerSqlErrorMessage))
            {
                errorMessage = innerSqlErrorMessage!;
            }
            else
            {
                var innerMsg = dbEx.InnerException?.Message?.ToLower() ?? "";

                if (innerMsg.Contains("duplicate key") || innerMsg.Contains("unique constraint"))
                {
                    errorMessage = _localizer[nameof(Resource.Db_Duplicate)];
                }
                else if (innerMsg.Contains("foreign key") || innerMsg.Contains("reference"))
                {
                    errorMessage = _localizer[nameof(Resource.Db_Reference)];
                }
                else if (innerMsg.Contains("concurrency"))
                {
                    errorMessage = _localizer[nameof(Resource.Db_Concurrency)];
                }
                else
                {
                    errorMessage = $"{_localizer[nameof(Resource.Db_Error)]}: {dbEx.Message}";
                }
            }
        }
        else
        {
            errorMessage = $"{_localizer[nameof(Resource.Generic_Exception)]}: {exception.Message}";
        }

        _logger.LogError(
            exception,
            "Mensaje final enviado al cliente: {ErrorMessage}",
            errorMessage
        );

        return Task.FromResult(new ActionResponse<T>
        {
            WasSuccess = false,
            Message = errorMessage,
            Result = default
        });
    }

    private bool TryGetSqlErrorMessage(Exception exception, out string? errorMessage)
    {
        errorMessage = null;
        string? typeName = exception.GetType().FullName;
        if (typeName is not ("Microsoft.Data.SqlClient.SqlException" or "System.Data.SqlClient.SqlException"))
        {
            return false;
        }

        object? errors = exception.GetType().GetProperty("Errors")?.GetValue(exception);
        if (errors is System.Collections.IEnumerable errorItems)
        {
            foreach (object error in errorItems)
            {
                object? numberValue = error.GetType().GetProperty("Number")?.GetValue(error);
                if (numberValue is int number && TryGetSqlErrorMessage(number, out errorMessage))
                {
                    return true;
                }
            }
        }

        object? exceptionNumberValue = exception.GetType().GetProperty("Number")?.GetValue(exception);
        if (exceptionNumberValue is int exceptionNumber && TryGetSqlErrorMessage(exceptionNumber, out errorMessage))
        {
            return true;
        }

        errorMessage = $"{_localizer[nameof(Resource.Db_Error)]}: {exception.Message}";
        return true;
    }

    private bool TryGetSqlErrorMessage(int sqlErrorNumber, out string? errorMessage)
    {
        errorMessage = sqlErrorNumber switch
        {
            2601 or 2627 => _localizer[nameof(Resource.Db_Duplicate)].Value,
            547 => _localizer[nameof(Resource.Db_Reference)].Value,
            1205 => _localizer["Db_Deadlock"].Value,
            -2 => _localizer["Generic_Timeout"].Value,
            4060 or 18456 => _localizer["Db_LoginFail"].Value,
            _ => null
        };

        return errorMessage != null;
    }
}
