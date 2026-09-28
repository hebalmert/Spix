namespace Spix.DomainLogic.Validations;

//El neutro [Select X] de un combo llega como Guid.Empty o 0.
//
//En un combo OPCIONAL eso significa "sin seleccionar", y tiene que viajar como null:
//si se manda Guid.Empty, la base lo lee como una llave foranea que no existe y rechaza
//el guardado, aunque el campo sea opcional.
public static class ComboValue
{
    public static Guid? OrNull(Guid value) => value == Guid.Empty ? null : value;

    public static int? OrNull(int value) => value == 0 ? null : value;
}
