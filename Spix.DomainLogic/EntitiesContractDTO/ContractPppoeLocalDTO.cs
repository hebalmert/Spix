using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesContractDTO;

public sealed class ContractPppoeLocalSetupDTO
{
    public Guid ContractClientId { get; set; }
    public Guid ServerId { get; set; }
    public Guid IpNetId { get; set; }
    public string? ServerName { get; set; }
    public string? ServerIp { get; set; }
    public string? ServerUser { get; set; }
    public string? ServerPassword { get; set; }
    public int ApiPort { get; set; }
    public string? ClientIp { get; set; }
    public string? ProfileName { get; set; }
    public string? ClientName { get; set; }
    public Guid? CredentialId { get; set; }
    public string? MikrotikId { get; set; }
    public string? CurrentUsername { get; set; }
    public string? CurrentPassword { get; set; }
    public PppoeAccessState AccessState { get; set; }
}

public sealed class ContractPppoeLocalSaveDTO
{
    public Guid ContractClientId { get; set; }
    public Guid? CredentialId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MikrotikId { get; set; } = string.Empty;
}
