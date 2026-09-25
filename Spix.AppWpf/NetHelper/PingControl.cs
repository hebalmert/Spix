// El ping del escritorio ya NO vive aqui.
//
// Estaba duplicado: esta copia y la de Spix.xNetwork/PingHelper, que es el proyecto
// transversal que ya usan el Backend y el propio WPF (LocalMikrotikService). Se dejo una
// sola, la del proyecto comun, y se le subieron las dos mejoras que tenia esta copia:
// las guardas de host vacio e intentos en cero, y soltar el objeto Ping al terminar.
//
// Ahora se usa: using Spix.xNetwork.PingHelper;
//
// Este archivo quedo vacio a proposito y se puede borrar.
