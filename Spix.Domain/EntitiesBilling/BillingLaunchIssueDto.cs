namespace Spix.Domain.EntitiesBilling;

//Un contrato que no se pudo facturar en el lanzamiento, con su motivo
public class BillingLaunchIssueDto
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string Reason { get; set; } = null!;
}
