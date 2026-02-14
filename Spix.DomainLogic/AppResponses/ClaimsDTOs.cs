namespace Spix.DomainLogic.AppResponses;

public class ClaimsDTOs
{
    public string SourceIp { get; set; } = default!;  //Ip de Origen
    public string UserAgent { get; set; } = default!; //Accion vino desde movil, desktop, o automatizacion
    public string Referer { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Role { get; set; } = default!;
    public int CorporationId { get; set; }
}