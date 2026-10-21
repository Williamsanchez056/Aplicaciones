# Aplicaciones

Proyecto TicTacToe multijugador con ASP.NET Core .NET 10, API REST y SignalR.

## Requisitos
- .NET 10 SDK
- Acceso a internet para cargar la biblioteca SignalR del navegador desde CDN

## Ejecutar
Desde la carpeta del proyecto:

```bash
dotnet run
```

Abre la URL que muestra la terminal. Para jugar desde dos computadoras, publica la aplicacion en un servidor accesible por ambas o usa una red local con el puerto permitido por el firewall. Los dos jugadores deben abrir la misma URL y usar el mismo identificador de partida. Uno selecciona X y el otro O.

## API
- `POST /api/Movimientos`: registra un movimiento y devuelve el estado actualizado.
- `GET /api/Movimientos/{partidaId}`: consulta el tablero y el historial de una partida.
- SignalR: `/partidaHub`; los clientes se unen a una partida y reciben el evento `MovimientoActualizado`.

Ejemplo de cuerpo para POST:

```json
{
  "partidaId": "partida-1",
  "fila": 0,
  "columna": 1,
  "jugador": "X"
}
```

Las filas y columnas van de 0 a 2. La partida se guarda en memoria del servidor; si la aplicacion se reinicia, se pierde el historial. El servidor valida los turnos, las casillas ocupadas y el fin de la partida.
