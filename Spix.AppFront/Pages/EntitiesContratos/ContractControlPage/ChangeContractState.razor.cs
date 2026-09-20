using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage;

public partial class ChangeContractState
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    [Parameter, EditorRequired] public Guid ContractClientId { get; set; }
    [Parameter, EditorRequired] public ContractState CurrentState { get; set; }

    private const string BaseUrl = "api/v1/contractcontrols";

    private List<IntItemModel>? States;
    private int NewState;
    private string? Motivo;
    private bool IsSaving;

    protected override async Task OnInitializedAsync()
    {
        //Las opciones validas desde el estado actual las decide el backend
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/loadStateChangeOptions/{ContractClientId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            States = new();
            return;
        }

        States = responseHttp.Response;
    }

    private void StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            NewState = value;
        }
    }

    private async Task SaveAsync()
    {
        if (NewState == 0)
        {
            await _sweetAlert.FireAsync(Localizer["ContractState_ChangeTitle"], Localizer["Validation_SelectStatus"], SweetAlertIcon.Warning);
            return;
        }

        var nombre = States!.First(x => x.Value == NewState).Name;

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["ContractState_ChangeTitle"],
            Text = string.Format(Localizer["ContractState_ChangeQuestion"], nombre),
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.ButtonSave)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirm.IsDismissed || confirm.Value != "true")
            return;

        //Se repinta a mano para que el spinner del boton se vea durante el proceso
        IsSaving = true;
        await InvokeAsync(StateHasChanged);

        //El backend vuelve a validar la transicion y el estado del MikroTik
        var responseHttp = await _repository.PutAsync<object, ContractClient>($"{BaseUrl}/changeState/{ContractClientId}/{NewState}?motivo={Uri.EscapeDataString(Motivo ?? string.Empty)}", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsSaving = false;
            return;
        }

        IsSaving = false;
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
