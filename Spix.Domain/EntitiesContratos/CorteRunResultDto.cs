namespace Spix.Domain.EntitiesContratos;

//El resultado de cortar un lote. Un contrato con problema no detiene el lote:
//se salta y se reporta aqui.
public class CorteRunResultDto
{
    public int Contracts { get; set; }

    public int Suspended { get; set; }

    //Los que ya no aplicaban: se pusieron al dia o alguien ya los suspendio
    public int Skipped { get; set; }

    public List<CorteRunIssueDto> Issues { get; set; } = new();
}

public class CorteRunIssueDto
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string Reason { get; set; } = null!;
}
