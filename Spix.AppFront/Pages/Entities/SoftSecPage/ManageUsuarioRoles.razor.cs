using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitesSoftSec;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Entities.SoftSecPage;

public partial class ManageUsuarioRoles
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 20;
    private const string BaseUrl = "api/v1/usuarioRoles";
    private bool HasChanges;
    private int SelectedUserType;
    private List<IntNameModel>? ListUserType;

    public Usuario? Usuario { get; set; }
    public List<UsuarioRole>? UsuarioRoles { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadRolesAsync();
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task LoadRolesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntNameModel>>($"{BaseUrl}/loadCombo");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        ListUserType = responseHttp.Response ?? new();
    }

    private void RoleChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            SelectedUserType = value;
    }

    private async Task AddRoleAsync()
    {
        if (SelectedUserType == 0)
        {
            await _sweetAlert.FireAsync("Rol", "Debe seleccionar un rol.", SweetAlertIcon.Warning);
            return;
        }

        var model = new UsuarioRole
        {
            UsuarioId = Id,
            UserType = (UserType)SelectedUserType
        };

        var responseHttp = await _repository.PostAsync(BaseUrl, model);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        HasChanges = true;
        SelectedUserType = 0;
        await LoadAsync(CurrentPage);
    }

    private async Task LoadAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<UsuarioRole>>($"{BaseUrl}?guidid={Id}&page={page}&recordsnumber={PageSize}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        UsuarioRoles = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        await LoadUsuarioAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadUsuarioAsync()
    {
        var responseHttp = await _repository.GetAsync<Usuario>($"/api/v1/usuarios/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Usuario = responseHttp.Response;
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        HasChanges = true;
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadAsync(CurrentPage);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(HasChanges ? ModalResult.Ok() : ModalResult.Cancel());
    }
}
