namespace Spix.Domain.EntitiesSchedule;

//El contrato como lo ve el cliente al pedir una visita: para reconocer cual es y nada mas
public class MyContractItemDto
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? Address { get; set; }

    public string? CityName { get; set; }

    public string? ZoneName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PlanName { get; set; }
}
