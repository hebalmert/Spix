using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.Domain.EntitiesBilling;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesBilling.SellPage;

//Muestra de que se compone una factura: el plan del mes y los servicios facturados.
//Los renglones ya vienen con el listado, asi que no se vuelve a pedir nada al servidor.
public partial class DetailsSell
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private ModalService ModalService { get; set; } = null!;

    [Parameter, EditorRequired] public Sell Sell { get; set; } = null!;
    [Parameter] public string? Title { get; set; }

    //De donde sale el renglon, en palabras
    private string OriginName(string? origin) => origin switch
    {
        "Plan" => Localizer[nameof(Resource.Plan)],
        "SolicitudServicio" => Localizer["Sell_OriginService"],
        _ => origin ?? string.Empty
    };

    private async Task Return()
    {
        await ModalService.CloseAsync(ModalResult.Cancel());
    }
}
