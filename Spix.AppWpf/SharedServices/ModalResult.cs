namespace Spix.AppWpf.SharedServices;

// Representa el resultado uniforme que los indices reciben al cerrar un modal.
public class ModalResult
{
    public bool Succeeded { get; set; }

    public static ModalResult Ok()
    {
        return new ModalResult
        {
            Succeeded = true
        };
    }

    public static ModalResult Cancel()
    {
        return new ModalResult
        {
            Succeeded = false
        };
    }
}
