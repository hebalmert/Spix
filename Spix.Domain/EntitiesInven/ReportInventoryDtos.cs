namespace Spix.Domain.EntitiesInven;

//Los seriales que hay de un producto y como estan repartidos
public class ReportSerialDto
{
    public string ProductName { get; set; } = null!;

    public int Total { get; set; }

    //Los que estan en la bodega listos para instalar
    public int Available { get; set; }

    //Los que estan puestos en un cliente
    public int Operative { get; set; }

    public int Damaged { get; set; }
}

//El total de seriales de la corporacion
public class ReportSerialSummaryDto
{
    public int Total { get; set; }

    public int Available { get; set; }

    public int Operative { get; set; }

    public int Damaged { get; set; }

    //Cuantos productos distintos tienen seriales
    public int Products { get; set; }
}
