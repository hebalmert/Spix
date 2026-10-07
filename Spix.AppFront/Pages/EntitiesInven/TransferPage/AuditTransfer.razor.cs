using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

//El historial de un traslado. El registro llega completo desde el index, asi que este
//modal solo pinta: no hay llamada al servidor ni spinner que mostrar.
public partial class AuditTransfer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter, EditorRequired] public Transfer Transfer { get; set; } = null!;

    //Sin fecha no se escribe un guion raro: se deja el espacio vacio
    private static string Fecha(DateTime? valor)
    {
        return valor is null ? string.Empty : valor.Value.ToString("dd/MM/yyyy HH:mm");
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
