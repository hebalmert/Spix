namespace Spix.Domain.EntitiesContratos;

//La revision previa del corte: quienes deben, a cuantos se les va a cortar, cuanto deben
//y como quedan repartidos por servidor. No cambia nada, solo cuenta.
public class CorteCheckDto
{
    //Todos los que deben algo, salidos de las cuentas por cobrar
    public int Debtors { get; set; }

    //De esos, los que ya estaban suspendidos: se dejan como estan
    public int AlreadySuspended { get; set; }

    public int ToSuspend { get; set; }

    public decimal DebtTotal { get; set; }

    //De los que se van a cortar, cuantos no tienen IpBinding
    public int NoServer { get; set; }

    //Este corte ya se ejecuto
    public bool Executed { get; set; }

    //El corte se lanza equipo por equipo: una conexion por servidor
    public List<CorteCheckServerDto> Servers { get; set; } = new();

}

//Un servidor con los contratos que se le van a cortar
public class CorteCheckServerDto
{
    //Vacio = contratos sin IpBinding, que no se pueden tocar en ningun equipo
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public int Contracts { get; set; }

    public decimal Debt { get; set; }
}
