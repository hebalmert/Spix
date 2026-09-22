using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMapPage;

//Modal que solo muestra el mapa; los textos por defecto los pone el MapViewer
public partial class ViewContractMap
{
    [Inject] private ModalService ModalService { get; set; } = null!;

    [Parameter] public decimal? Latitude { get; set; }
    [Parameter] public decimal? Longitude { get; set; }
    [Parameter] public decimal? SecondLatitude { get; set; }
    [Parameter] public decimal? SecondLongitude { get; set; }
    [Parameter] public string? FirstLabel { get; set; }
    [Parameter] public string? SecondLabel { get; set; }
    [Parameter] public string? Title { get; set; }

    private async Task Return()
    {
        await ModalService.CloseAsync(ModalResult.Cancel());
    }
}
