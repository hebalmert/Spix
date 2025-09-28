﻿using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Spix.DomainLogic.ResponcesSec;

namespace Spix.AppBack.Helper;

public static class ClaimsPrincipalExtensions
{
    public static ClaimsDTOs GetEmailOrThrow(this ClaimsPrincipal user, IStringLocalizer localizer)
    {
        if (user?.Identity?.IsAuthenticated != true)
            throw new ApplicationException(localizer["Generic_AuthRequired"].Value);

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

        if (string.IsNullOrWhiteSpace(email))
            throw new ApplicationException(localizer["Generic_AuthEmailFail"].Value);

        if (string.IsNullOrWhiteSpace(id))
            throw new ApplicationException(localizer["Generic_AuthIdFail"].Value);

        if (string.IsNullOrWhiteSpace(role))
            throw new ApplicationException(localizer["Generic_AuthRoleFail"].Value);

        return new ClaimsDTOs
        {
            UserName = email,
            Id = id,
            CorporationId = Idcorporate,
            Role = role
        };
    }
}