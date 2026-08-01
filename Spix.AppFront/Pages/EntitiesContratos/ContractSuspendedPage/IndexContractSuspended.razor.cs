using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Net;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractSuspendedPage;

public partial class IndexContractSuspended
{
    private const string ContractBindRequiredMessage = "El contrato no tiene un IpBinding activo configurado.";
    private const string HotSpotActivationRequirementsMessage = "MikroTik Hotspot";

    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> _localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractsuspended";
    private string Filter { get; set; } = string.Empty;
    private List<ContractSuspendedDTO> Contracts { get; set; } = new();

    private async Task SearchAsync(ChangeEventArgs e)
    {
        Filter = e.Value?.ToString()?.Trim() ?? string.Empty;

        if (Filter.Length < 2)
        {
            Contracts.Clear();
            return;
        }

        var responseHttp = await _repository.GetAsync<List<ContractSuspendedDTO>>(
            $"{BaseUrl}?filter={Uri.EscapeDataString(Filter)}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Contracts = responseHttp.Response ?? new();
        }
    }

    private Task ClearAsync()
    {
        Filter = string.Empty;
        Contracts.Clear();
        return Task.CompletedTask;
    }

    private async Task ActivateAsync(ContractSuspendedDTO contract)
    {
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Activar contrato",
            Text = $"Desea activar el contrato {contract.ControlContrato} de {contract.ClientFullName}?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Activar",
            CancelButtonText = "Cancelar"
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{contract.ContractClientId}/activate", new { });

        if (responseHttp.HttpResponseMessage?.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage = await responseHttp.GetErrorMessageAsync();
            var mikrotikConnectionMessage = _localizer[nameof(Resource.Mikrotik_Connection_Error)].Value;

            if (string.Equals(errorMessage, mikrotikConnectionMessage, StringComparison.OrdinalIgnoreCase))
            {
                await _sweetAlert.FireAsync(
                    "No se pudo conectar con MikroTik",
                    "No fue posible activar el acceso remoto. El contrato continuara suspendido.",
                    SweetAlertIcon.Warning);
                return;
            }

            if (errorMessage?.Contains(HotSpotActivationRequirementsMessage, StringComparison.OrdinalIgnoreCase) == true)
            {
                await _sweetAlert.FireAsync(
                    "Contrato pendiente de configuracion MikroTik",
                    "La corporacion maneja MikroTik HotSpot y el contrato aun no tiene Contract Queue e IpBinding. No puede retirarse de Suspendidos ni pasar a Active.",
                    SweetAlertIcon.Warning);
                return;
            }

        }

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Contracts.Remove(contract);
        await _sweetAlert.FireAsync(
            "Contrato activado",
            "El contrato fue activado correctamente.",
            SweetAlertIcon.Success);
    }
}
