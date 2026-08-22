using Spix.DomainLogic.AppResponses;

namespace Spix.DomainLogic.ModelUtility;

public class RefreshSessionDTO
{
    public TokenDTO AccessToken { get; set; } = new();
    public string RefreshToken { get; set; } = string.Empty;
}
