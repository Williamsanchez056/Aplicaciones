using System.Collections.Concurrent;
using Aplicaciones.Shared.Contracts;

namespace Aplicaciones.Services;

public sealed class PartidaService
{
    private readonly ConcurrentDictionary<string, Partida> _partidas = new();

    public PartidaResponse AgregarMovimiento(MovimientoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PartidaId))
            throw new ArgumentException("El identificador de la partida es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Jugador))
            throw new ArgumentException("El jugador es obligatorio.");
        if (request.Fila is < 0 or > 2 || request.Columna is < 0 or > 2)
            throw new ArgumentException("La fila y la columna deben estar entre 0 y 2.");

        var partidaId = request.PartidaId.Trim();
        var jugador = request.Jugador.Trim().ToUpperInvariant();
        if (partidaId.Length > 50)
            throw new ArgumentException("El identificador no puede superar 50 caracteres.");
        if (jugador is not ("X" or "O"))
            throw new ArgumentException("El jugador debe ser X u O.");

        var partida = _partidas.GetOrAdd(partidaId, id => new Partida(id));
        lock (partida)
        {
            if (partida.Ganador is not null || partida.Empate)
                throw new InvalidOperationException("La partida ya termino.");
            if (jugador != partida.JugadorActual)
                throw new InvalidOperationException($"Es el turno de {partida.JugadorActual}.");
            if (partida.Tablero[request.Fila][request.Columna] != "")
                throw new InvalidOperationException("Esa casilla ya esta ocupada.");

            partida.Tablero[request.Fila][request.Columna] = jugador;
            partida.Movimientos.Add(new Movimiento(
                partida.Movimientos.Count + 1, partida.Id, request.Fila,
                request.Columna, jugador, jugador, DateTimeOffset.UtcNow));

            if (HayGanador(partida.Tablero, jugador))
                partida.Ganador = jugador;
            else if (partida.Movimientos.Count == 9)
                partida.Empate = true;
            else
                partida.JugadorActual = jugador == "X" ? "O" : "X";

            return CrearRespuesta(partida);
        }
    }

    public PartidaResponse ObtenerPartida(string partidaId)
    {
        if (string.IsNullOrWhiteSpace(partidaId))
            throw new ArgumentException("El identificador de la partida es obligatorio.");
        if (!_partidas.TryGetValue(partidaId.Trim(), out var partida))
            throw new KeyNotFoundException("No existe una partida con ese identificador.");

        lock (partida)
            return CrearRespuesta(partida);
    }

    private static PartidaResponse CrearRespuesta(Partida partida) =>
        new(partida.Id, partida.Tablero.Select(fila => fila.ToArray()).ToArray(),
            partida.JugadorActual, partida.Ganador, partida.Empate,
            partida.Movimientos.ToArray());

    private static bool HayGanador(string[][] tablero, string simbolo)
    {
        for (var i = 0; i < 3; i++)
        {
            if (tablero[i].All(casilla => casilla == simbolo)) return true;
            if (tablero.All(fila => fila[i] == simbolo)) return true;
        }
        return (tablero[0][0] == simbolo && tablero[1][1] == simbolo && tablero[2][2] == simbolo)
            || (tablero[0][2] == simbolo && tablero[1][1] == simbolo && tablero[2][0] == simbolo);
    }

    private sealed class Partida(string id)
    {
        public string Id { get; } = id;
        public string[][] Tablero { get; } =
        [
            ["", "", ""],
            ["", "", ""],
            ["", "", ""]
        ];
        public string JugadorActual { get; set; } = "X";
        public string? Ganador { get; set; }
        public bool Empate { get; set; }
        public List<Movimiento> Movimientos { get; } = [];
    }
}
