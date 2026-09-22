namespace Spix.DomainLogic.EnumTypes;

//Como se pinta el Mapa de nodos
public enum NodeMapViewType
{
    //El AP y los clientes, sin lineas
    Points = 1,

    //Una linea del AP a cada cliente con ubicacion
    Lines = 2,

    //Las lineas con la distancia de cada cliente encima
    LinesWithDistance = 3
}
