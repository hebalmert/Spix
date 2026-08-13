using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Serial;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Serial;

// Lista las MAC de la corporacion con la misma consulta global usada por Cargue Seriales en Blazor.
public partial class SerialIndexViewModel : PagedListViewModel<CargueDetail>
{
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    protected override string Endpoint => "api/v1/cargueDetails/GetSerials";

    public SerialIndexViewModel(
        IPagedEntityService<CargueDetail> pagedEntityService,
        ModalService modalService,
        AlertService alertService)
        : base(pagedEntityService)
    {
        _modalService = modalService;
        _alertService = alertService;
    }

    // Mantiene la regla existente: una MAC operativa no se edita desde este modulo.
    [RelayCommand]
    private async Task EditAsync(CargueDetail? serial)
    {
        if (serial is null || serial.Status == SerialStateType.Operativo)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = serial.CargueDetailId
        };

        var result = await _modalService.ShowAsync<EditSerialDialogView>(
            "Editar serial",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El serial fue actualizado correctamente.");
    }
}
