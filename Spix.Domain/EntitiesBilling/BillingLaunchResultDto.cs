namespace Spix.Domain.EntitiesBilling;

//El resultado de lanzar las notas de cobro generales: que se hizo y que quedo pendiente.
//Un contrato con problema no detiene el lote: se salta y se reporta aqui.
public class BillingLaunchResultDto
{
    public int Contracts { get; set; }

    public int Created { get; set; }

    //Los que ya tenian nota del periodo
    public int Skipped { get; set; }

    public List<BillingLaunchIssueDto> Issues { get; set; } = new();
}
