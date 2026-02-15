using Microsoft.Extensions.Localization;
using Spix.DomainLogic.AppResponses;
using Spix.xLanguage.Resources;
using System.Security.Claims;

namespace Spix.AppBack.Helper;

public static class ClaimsPrincipalExtensions
{
    public static ClaimsDTOs GetEmailOrThrow(this ClaimsPrincipal user, IStringLocalizer localizer, HttpContext httpContext)
    {
        if (user?.Identity?.IsAuthenticated != true)
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthRequired)]);

        int Idcorporate;
        string? email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        string? id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        string? role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (role == "Admin")
        {
            Idcorporate = 0;
        }
        else
        {
            Idcorporate = Convert.ToInt32(user.Claims.FirstOrDefault(c => c.Type == "CorporateId")?.Value);
        }

        string ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        string userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        string referer = httpContext.Request.Headers["Referer"].ToString();

        if (string.IsNullOrWhiteSpace(email))
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthEmailFail)].Value);

        if (string.IsNullOrWhiteSpace(id))
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthIdFail)]);

        if (string.IsNullOrWhiteSpace(role))
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthRoleFail)]);

        return new ClaimsDTOs
        {
            UserName = email,
            Id = id,
            CorporationId = Idcorporate,
            Role = role,
            SourceIp = ip,
            UserAgent = userAgent,
            Referer = referer
        };
    }
}