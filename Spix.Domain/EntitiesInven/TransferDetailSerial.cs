using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesInven;

// Que equipos movio cada linea del traslado.
//
// Hace falta una tabla aparte porque el serial solo sabe en que bodega esta AHORA: al
// cerrar el traslado pasa a la bodega destino y suelta la reserva, asi que sin esto no
// quedaba forma de saber que equipos viajaron en cada traslado.
//
// Se escribe UNA sola vez, al cerrar el traslado, y no se vuelve a tocar: es historico.
// Por eso el mismo equipo puede figurar en varios traslados sin pisarse.
public class TransferDetailSerial
{
    [Key]
    public Guid TransferDetailSerialId { get; set; }

    public Guid TransferDetailsId { get; set; }

    public Guid CargueDetailId { get; set; }

    // La MAC queda CONGELADA, igual que FromStorageName o ReceivedByName: si mañana se
    // borra el serial, el traslado viejo sigue diciendo que equipo se movio.
    [MaxLength(50)]
    [Display(Name = nameof(Resource.MAC), ResourceType = typeof(Resource))]
    public string? MacWlan { get; set; }

    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime DateMoved { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public TransferDetails? TransferDetails { get; set; }
    public CargueDetail? CargueDetail { get; set; }
}
