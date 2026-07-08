using Microsoft.Extensions.Localization;
using Spix.DomainLogic.AppResponses;
using Spix.xLanguage.Resources;
using System.Security.Claims;

namespace Spix.AppBack.Helper;

public static class ClaimsPrincipalExtensions
{
    public static ClaimsDTOs GetEmailOrThrow(this ClaimsPrincipal user, IStringLocalizer localizer, HttpContext httpContext)
        => user.GetSecurityContextOrThrow(localizer, httpContext);

    public static ClaimsDTOs GetSecurityContextOrThrow(this ClaimsPrincipal user, IStringLocalizer localizer, HttpContext httpContext)
    {
        if (user?.Identity?.IsAuthenticated != true)
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthRequired)]);

        string? username = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        string? id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        string[] roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();
        string? role = roles.FirstOrDefault();
        string? corporateIdValue = user.Claims.FirstOrDefault(c => c.Type == "CorporateId")?.Value;

        string forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? string.Empty;
        string ip = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        string userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        string referer = httpContext.Request.Headers["Referer"].ToString();

        if (string.IsNullOrWhiteSpace(username))
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthUserNameFail)]);

        if (string.IsNullOrWhiteSpace(id))
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthIdFail)]);

        if (string.IsNullOrWhiteSpace(role))
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthRoleFail)]);

        int corporationId = 0;
        if (!roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) &&
            !int.TryParse(corporateIdValue, out corporationId))
        {
            throw new ApplicationException(localizer[nameof(Resource.Generic_AuthIdFail)]);
        }

        return new ClaimsDTOs
        {
            UserName = username!,
            Id = id,
            CorporationId = corporationId,
            Role = role,
            SourceIp = ip,
            UserAgent = userAgent,
            Referer = referer
        };
    }
}
