using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesInvenDTO;

//Un cargue en el listado, con su avance ya contado por la base
public class CargueListItemDto
{
    public Guid CargueId { get; set; }

    public string? ControlCargue { get; set; }

    public DateTime DateCargue { get; set; }

    public string? NroFactura { get; set; }

    public string? ProductName { get; set; }

    //Lo que dice la compra que hay que subir
    public decimal CantToUp { get; set; }

    //Lo que ya se subio
    public int Uploaded { get; set; }

    public CargueType Status { get; set; }
}

//Los numeros del tablero del inventario, sobre toda la corporacion
public class CargueSummaryDto
{
    public int PendingCargues { get; set; }

    //Lo que falta por subir en los cargues abiertos
    public int ToUpload { get; set; }

    public int Available { get; set; }

    public int Installed { get; set; }

    public int Damaged { get; set; }
}

//El avance de UN cargue, con el desglose de sus seriales
public class CargueProgressDto
{
    public Guid CargueId { get; set; }

    public string? ControlCargue { get; set; }

    public DateTime DateCargue { get; set; }

    public string? NroFactura { get; set; }

    public string? ProductName { get; set; }

    public decimal CantToUp { get; set; }

    public int Uploaded { get; set; }

    public int Available { get; set; }

    public int Installed { get; set; }

    public int Damaged { get; set; }

    public CargueType Status { get; set; }
}

//Un serial del cargue, con el contrato donde quedo instalado si ya se uso
public class CargueSerialDto
{
    public Guid CargueDetailId { get; set; }

    public string? MacWlan { get; set; }

    public string? Comment { get; set; }

    public SerialStateType Status { get; set; }

    public DateTime? DateCargue { get; set; }

    //Sale del contrato al que se asigno la MAC, no del comentario
    public long? ControlContrato { get; set; }

    public string? InstalledClient { get; set; }
}
