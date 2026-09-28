namespace Spix.DomainLogic.MkDTOs;

//Una interfaz leida del propio MikroTik.
//
//Name es el valor que viaja al equipo (ether1, sfp-sfpplus1, bridge-clientes) y Text es lo
//que se pinta. La lista se arma COMPLETA en el backend, con el neutro traducido en la
//posicion 0: el front solo pone value y @onchange, no filtra ni inserta opciones.
public class MkInterfaceDTO
{
    public string Name { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}
