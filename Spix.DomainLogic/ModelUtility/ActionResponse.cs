namespace Spix.DomainLogic.ModelUtility;

public class ActionResponse<T>
{
    public bool WasSuccess { get; set; }

    public string? Message { get; set; }

    public T? Result { get; set; }
}