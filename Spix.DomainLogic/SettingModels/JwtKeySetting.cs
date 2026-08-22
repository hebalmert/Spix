namespace Spix.DomainLogic.SettingModels;

public class JwtKeySetting
{
    public string? jwtKey { get; set; }
    public int RefreshTokenExpirationDays { get; set; } = 1;
}
