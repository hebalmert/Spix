namespace Spix.DomainLogic.EnumTypes;

//El estado del acceso de un cliente PPPoE, espejo de lo que quedo en el equipo.
//
//NO es una tabla catalogo como HotSpotType. Esa es tabla porque su TypeName viaja literal
//al MikroTik en "=type=regular". Aca lo que viaja es "=disabled=yes/no" y el nombre del
//perfil, asi que no hay ningun texto que haya que sembrar en la base.
public enum PppoeAccessState
{
    //El secret habilitado: el cliente puede autenticar y navegar
    Activo = 1,

    //El secret deshabilitado por mora, y su sesion tumbada
    Corte = 2,

    //Bloqueado a mano, fuera del ciclo de cobro
    Bloqueado = 3,
}
