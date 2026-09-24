namespace Spix.Domain.EntitiesContratos;

//Los contratos que entraron en el periodo y lo que representan al mes
public class ReportContractsSummaryDto
{
    public int NewContracts { get; set; }

    //De esos, los que ya quedaron dando servicio
    public int Installed { get; set; }

    //Lo que suman los planes de los que quedaron activos: es lo que se agrego a la
    //facturacion mensual
    public decimal MonthlyTotal { get; set; }
}

//Un plan y cuantas veces se contrato en el periodo
public class ReportPlanDto
{
    public string Name { get; set; } = null!;

    public int Times { get; set; }

    public decimal MonthlyTotal { get; set; }
}

//Lo que paso con las solicitudes de servicio en el periodo
public class ReportServicesSummaryDto
{
    //Todas las que entraron, sin importar como terminaron
    public int Requested { get; set; }

    //Las que se resolvieron llamando, sin mandar a nadie
    public int PhoneResolved { get; set; }

    //Las que se le pusieron fecha a un tecnico
    public int Scheduled { get; set; }

    public int Completed { get; set; }

    public int Cancelled { get; set; }

    //Lo que se alcanzo a cobrar por esas solicitudes
    public decimal Billed { get; set; }
}

//Un tipo de servicio y cuantas veces se hizo
public class ReportServiceDto
{
    public string Name { get; set; } = null!;

    public string? CategoryName { get; set; }

    public int Times { get; set; }

    //Lo que se cobro por ese servicio en el periodo
    public decimal Total { get; set; }
}

//Un tecnico y cuantos servicios resolvio
public class ReportTechnicianDto
{
    public string Name { get; set; } = null!;

    public int Services { get; set; }

    public decimal Total { get; set; }
}

//Como le fue al corte: cuantos se cortaron y cuantos pagaron por eso
public class ReportCutOffSummaryDto
{
    public int Cut { get; set; }

    //Lo que facturan al mes los contratos que se cortaron
    public decimal CutAmount { get; set; }

    //De los cortados, los que ya pagaron
    public int Recovered { get; set; }

    public decimal RecoveredAmount { get; set; }

    //Los que ya se volvieron a montar en la Mikrotik
    public int Reactivated { get; set; }

    //Los que siguen abajo
    public int StillDown { get; set; }

    public decimal StillDownAmount { get; set; }
}

//Como le fue al corte en una zona
public class ReportCutOffZoneDto
{
    public string Name { get; set; } = null!;

    public int Cut { get; set; }

    public int Recovered { get; set; }

    public decimal Amount { get; set; }
}

//Los contratos que ya no dan servicio y lo que dejaron de facturar
public class ReportChurnDto
{
    public int Terminated { get; set; }

    public decimal TerminatedAmount { get; set; }

    public int Cancelled { get; set; }

    public decimal CancelledAmount { get; set; }

    //Los que estan suspendidos hoy: todavia no son bajas, pero no estan pagando
    public int Suspended { get; set; }

    public decimal SuspendedAmount { get; set; }
}

//Las bajas de una zona
public class ReportChurnZoneDto
{
    public string Name { get; set; } = null!;

    public int Contracts { get; set; }

    public decimal Amount { get; set; }
}
