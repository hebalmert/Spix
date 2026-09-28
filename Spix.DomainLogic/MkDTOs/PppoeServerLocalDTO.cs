namespace Spix.DomainLogic.MkDTOs;

public sealed class PppoeServerLocalSetupDTO
{
    public Guid ServerId { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string ServerIp { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int ApiPort { get; set; }
    public string LanName { get; set; } = string.Empty;
    public string LocalIp { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public bool IsProvisioned { get; set; }
}

public sealed class PppoeServerLocalSaveDTO
{
    public Guid ServerId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string ProfileMikrotikId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServerMikrotikId { get; set; } = string.Empty;
}

//Lo que el escritorio necesita para BORRAR la configuracion PPPoE del equipo: como
//llegar al router y los dos .id que Spix escribio al crearla. Sin los .id no se borra
//nada: si Spix no tiene el id de algo, ese algo no es de Spix.
public sealed class PppoeServerLocalRemoveDTO
{
    public Guid ServerId { get; set; }
    public string ServerIp { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int ApiPort { get; set; }
    public string ProfileMikrotikId { get; set; } = string.Empty;
    public string ServerMikrotikId { get; set; } = string.Empty;
}
