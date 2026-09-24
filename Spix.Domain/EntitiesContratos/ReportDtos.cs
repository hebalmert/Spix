namespace Spix.Domain.EntitiesContratos;

//Un contrato activo con lo que paga: el renglon del reporte
public class ReportActiveContractDto
{
    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? ZoneName { get; set; }

    public string? ServerName { get; set; }

    public string? NodeName { get; set; }

    //Para cuando el reporte muestra todos, no solo los activos
    public bool IsActive { get; set; }

    public string? PlanName { get; set; }

    public decimal PlanPrice { get; set; }
}

//Los totales del reporte de contratos activos
public class ReportActiveSummaryDto
{
    public int Contracts { get; set; }

    //Lo que factura el mes si todos pagan su plan
    public decimal MonthlyTotal { get; set; }

    //Los que estan activos pero no tienen plan configurado
    public int WithoutPlan { get; set; }
}

//Una fila de los reportes agrupados: por zona o por servidor
public class ReportGroupDto
{
    public string Name { get; set; } = null!;

    public int Contracts { get; set; }

    public decimal MonthlyTotal { get; set; }
}
