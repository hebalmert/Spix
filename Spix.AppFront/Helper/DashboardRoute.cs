using Spix.DomainLogic.EnumTypes;
using System.Security.Claims;

namespace Spix.AppFront.Helper;

//A que dashboard le toca a cada rol. Vivia copiado en Login y en LoginPage; ahora lo necesita
//tambien UnauthorizedRedirect, asi que la cascada de roles queda en un solo sitio.
public static class DashboardRoute
{
    public static string For(IEnumerable<string> roles)
    {
        if (roles.Any(x => string.Equals(x, UserType.Admin.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            return "/saasdashboard";
        }

        if (roles.Any(x => string.Equals(x, UserType.Client.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            return "/client-dashboard";
        }

        if (roles.Any(x => string.Equals(x, UserType.Technician.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            return "/tech-dashboard";
        }

        if (roles.Any(x => string.Equals(x, UserType.Contractor.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            return "/contractor-dashboard";
        }

        return "/dashboard";
    }

    //Los roles del usuario logueado. El token los trae con el tipo largo de ClaimTypes.Role,
    //pero se acepta tambien el corto para no depender de como se escribio el token.
    public static List<string> RolesOf(ClaimsPrincipal user)
    {
        return user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();
    }
}
