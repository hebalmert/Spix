namespace Spix.Domain.EntitiesContratos;

//Un contrato que quedo cortado, como se muestra en la lista del corte ya ejecutado
public class CorteDetailDto
{
    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? CollectionNote { get; set; }

    public decimal Debt { get; set; }

    public decimal PlanAmount { get; set; }
}
