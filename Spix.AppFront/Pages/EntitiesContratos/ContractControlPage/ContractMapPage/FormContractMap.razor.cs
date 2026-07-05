using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesContratos;
using System.Globalization;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMapPage;

public partial class FormContractMap
{
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    [Parameter, EditorRequired] public ContractMap ContractMap { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private string CoordinatesText { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        if (ContractMap.Latitude.HasValue && ContractMap.Longitude.HasValue && string.IsNullOrWhiteSpace(CoordinatesText))
        {
            CoordinatesText = $"{ContractMap.Latitude.Value.ToString(CultureInfo.InvariantCulture)}, {ContractMap.Longitude.Value.ToString(CultureInfo.InvariantCulture)}";
        }
    }

    private async Task CoordinatesChanged(ChangeEventArgs e)
    {
        CoordinatesText = e.Value?.ToString() ?? string.Empty;
        var parts = CoordinatesText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var latitude) ||
            !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var longitude))
        {
            await _sweetAlert.FireAsync("Coordenadas", "Formato invalido. Use: 25.82370270482433, -80.38556718743175", SweetAlertIcon.Warning);
            return;
        }

        ContractMap.Latitude = latitude;
        ContractMap.Longitude = longitude;
    }
}
