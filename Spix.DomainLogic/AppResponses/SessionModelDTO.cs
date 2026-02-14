namespace Spix.DomainLogic.AppResponses;

public class SessionModelDTO
{
    public DateTime? Expiration { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? LogoBase64 { get; set; }
    public string? Role { get; set; }
}