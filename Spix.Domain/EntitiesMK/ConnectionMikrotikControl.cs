using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;

namespace Spix.Domain.EntitiesMK;

public class ConnectionMikrotikControl
{
    public Guid ConnectionMikrotikControlId { get; set; }

    public MikrotikControlType MikrotikControlType { get; set; } = MikrotikControlType.Ninguno;

    public int CorporationId { get; set; }
    public Corporation? Corporation { get; set; }
}
