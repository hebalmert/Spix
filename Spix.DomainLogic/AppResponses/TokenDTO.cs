namespace Spix.DomainLogic.AppResponses;

public class TokenDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? LogoBase64 { get; set; }
}