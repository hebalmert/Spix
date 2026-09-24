using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesEmails.EmailProvider;

// Los campos de la configuracion de correo. Los que se piden dependen del proveedor:
// SendGrid pide su llave; Gmail pide servidor, puerto, usuario y clave.
public abstract partial class EmailProviderFormViewModel : CrudFormViewModel<EmailProviderSetting>
{
    [ObservableProperty]
    private ObservableCollection<EmailProviderType> _providerTypes =
        new(Enum.GetValues<EmailProviderType>());

    protected override string BaseUrl => "api/v1/emailproviders";

    protected EmailProviderFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override EmailProviderSetting CreateEntity()
    {
        return new EmailProviderSetting
        {
            ProviderType = EmailProviderType.SendGrid,
            SmtpUseSsl = true,
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.Name))
        {
            return "Debes ingresar el nombre de la configuracion.";
        }

        if (string.IsNullOrWhiteSpace(Entity.FromEmail))
        {
            return "Debes ingresar el correo del remitente.";
        }

        //La entidad tiene [EmailAddress]: si no se revisa aqui, el error llega del
        //servidor y el usuario lo ve despues de esperar la peticion
        if (!new EmailAddressAttribute().IsValid(Entity.FromEmail))
        {
            return "El correo del remitente no es valido.";
        }

        if (string.IsNullOrWhiteSpace(Entity.FromName))
        {
            return "Debes ingresar el nombre del remitente.";
        }

        if (Entity.ProviderType == EmailProviderType.SendGrid)
        {
            //Al editar la llave no vuelve a bajar: solo se exige cuando es nueva
            if (Entity.EmailProviderSettingId == Guid.Empty && string.IsNullOrWhiteSpace(Entity.SendGridApiKey))
            {
                return "Debes ingresar la llave de SendGrid.";
            }

            return null;
        }

        if (string.IsNullOrWhiteSpace(Entity.SmtpHost))
        {
            return "Debes ingresar el servidor de correo.";
        }

        if (Entity.SmtpPort is null or <= 0)
        {
            return "Debes ingresar el puerto del servidor.";
        }

        if (string.IsNullOrWhiteSpace(Entity.SmtpUser))
        {
            return "Debes ingresar el usuario del correo.";
        }

        return null;
    }

    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
    }
}

public partial class CreateEmailProviderDialogViewModel : EmailProviderFormViewModel
{
    public CreateEmailProviderDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditEmailProviderDialogViewModel : EmailProviderFormViewModel
{
    public EditEmailProviderDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
