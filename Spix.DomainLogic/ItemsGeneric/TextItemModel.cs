namespace Spix.DomainLogic.ItemsGeneric;

//Un item de combo cuya llave es TEXTO, no un int ni un Guid.
//Lo usan las listas que mezclan dos origenes y necesitan decir de cual vienen,
//por ejemplo "T:<guid>" para un tecnico y "U:<guid>" para un usuario.
public class TextItemModel
{
    public string? Name { get; set; }

    public string Value { get; set; } = string.Empty;
}
