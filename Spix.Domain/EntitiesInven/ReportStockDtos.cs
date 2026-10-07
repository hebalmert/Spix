namespace Spix.Domain.EntitiesInven;

//Un movimiento de inventario.
//
//NO hay tabla de movimientos: el Stock es solo un acumulado. Estas filas se deducen de los
//documentos que si lo mueven, que son la compra cerrada y el traslado cerrado. La ventaja
//es que el reporte sirve hacia atras, con lo que ya esta registrado.
public class ReportStockMoveDto
{
    public DateTime Date { get; set; }

    //Compra o Traslado
    public string Origin { get; set; } = null!;

    //El numero del documento que lo movio
    public string Document { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string StorageName { get; set; } = null!;

    //Positiva si entra a la bodega, negativa si sale
    public decimal Quantity { get; set; }

    //De quien viene el movimiento: el PROVEEDOR en la compra, el USUARIO en el
    //traslado. La compra no guarda usuario, asi que no se puede unificar en uno solo.
    public string Reference { get; set; } = string.Empty;

    public bool IsEntry => Quantity >= 0;
}

//Lo que hay HOY de un producto en una bodega, con el desglose de sus seriales.
//Los seriales solo se pueden mostrar como foto: el sistema guarda su estado pero no
//cuando cambio, asi que no hay historia de esos movimientos.
public class ReportStockBalanceDto
{
    public string ProductName { get; set; } = null!;

    public string StorageName { get; set; } = null!;

    public decimal Stock { get; set; }

    public bool WithSerials { get; set; }

    public int Available { get; set; }

    public int Operative { get; set; }

    public int Damaged { get; set; }
}

//Los numeros de arriba del reporte
public class ReportStockSummaryDto
{
    public decimal Entries { get; set; }

    public decimal Exits { get; set; }

    public int Moves { get; set; }

    //Cuantas bodegas tienen existencia
    public int Storages { get; set; }

    public decimal TotalStock { get; set; }
}
