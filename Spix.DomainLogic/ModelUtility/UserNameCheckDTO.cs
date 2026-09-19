namespace Spix.DomainLogic.ModelUtility;

//Respuesta de la revision de un nombre de usuario contra Identity.
//Si esta ocupado se devuelven alternativas libres, como hace Google al crear un correo.
public class UserNameCheckDTO
{
    public string UserName { get; set; } = string.Empty;

    public bool Available { get; set; }

    public List<string> Suggestions { get; set; } = new();
}
