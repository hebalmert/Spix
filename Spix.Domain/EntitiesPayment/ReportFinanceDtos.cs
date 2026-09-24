namespace Spix.Domain.EntitiesPayment;

//Lo que recogio cada quien en el periodo: una fila por persona
public class ReportCollectorDto
{
    public string Name { get; set; } = null!;

    public int Collections { get; set; }

    public decimal Total { get; set; }
}

//El recaudo del periodo, separado por como entro la plata
public class ReportCollectionSummaryDto
{
    public int Collections { get; set; }

    //Lo que de verdad entro
    public decimal Total { get; set; }

    public decimal Cash { get; set; }

    public decimal Card { get; set; }

    public decimal Transfer { get; set; }

    //Lo que se cruzo con un pago adelantado: ya habia entrado antes
    public decimal PrePayment { get; set; }

    //Lo perdonado: no es plata que entro, pero explica por que la nota quedo en cero
    public decimal Discount { get; set; }
}

//Las notas de cobro emitidas en el periodo
public class ReportNotesSummaryDto
{
    public int Notes { get; set; }

    public decimal Issued { get; set; }

    public decimal Collected { get; set; }

    public decimal Pending { get; set; }
}

//La cartera que esta viva, repartida por lo vieja que es la nota
public class ReportAgingDto
{
    public int Notes { get; set; }

    public decimal Balance { get; set; }

    //Del mes: 30 dias o menos
    public int NotesCurrent { get; set; }

    public decimal Current { get; set; }

    //Entre 31 y 60 dias
    public int Notes30 { get; set; }

    public decimal Days30 { get; set; }

    //Entre 61 y 90 dias
    public int Notes60 { get; set; }

    public decimal Days60 { get; set; }

    //Mas de 90 dias: la plata que ya casi no se cobra
    public int Notes90 { get; set; }

    public decimal Days90 { get; set; }
}

//Un contrato que debe: cuanto arrastra y desde cuando
public class ReportDebtorDto
{
    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? ZoneName { get; set; }

    public int Notes { get; set; }

    public decimal Balance { get; set; }

    //La nota mas vieja sin pagar
    public DateTime OldestNote { get; set; }

    public int Days { get; set; }
}

//Lo que se le causo a un contratista en el periodo y lo que se le queda debiendo
public class ReportContractorCommissionDto
{
    public string Name { get; set; } = null!;

    public int Commissions { get; set; }

    //Sobre cuanta plata cobrada se calculo
    public decimal BaseAmount { get; set; }

    public decimal Total { get; set; }

    public decimal Paid { get; set; }

    public decimal Balance { get; set; }
}

//Un movimiento de la bitacora del dinero
public class ReportAuditDto
{
    public DateTime DateEvent { get; set; }

    public int EventType { get; set; }

    public string? EventName { get; set; }

    public decimal Total { get; set; }

    public string? Detail { get; set; }

    public string? UserByName { get; set; }

    public string? ClientFullName { get; set; }
}
