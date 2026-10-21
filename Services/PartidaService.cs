using System.Collections.Concurrent;
using Aplicaciones.Models;

namespace Aplicaciones.Services;

public sealed class PartidaService
{
    private readonly ConcurrentDictionary<string, Partida> _partidas = new();

    public PartidaResponse AgregarMovimiento(MovimientoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PartidaId))
            throw new ArgumentException("El identificador de la partida es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Jugador))
            throw new ArgumentException("El nombre del jugador es obligatorio.");

        if (request.Fila is < 0 or > 2 || request.Columna is < 0 or > 2)
            throw new ArgumentException("La fila y la columna deben estar entre 0 y 2.");

        var partidaId = request.PartidaId.Trim();
        var jugador = request.Jugador.Trim();

        if (jugador.Length > 40)
            throw new ArgumentException("El nombre del jugador no puede superar 40 caracteres.");

        var partida = _partidas.GetOrAdd(partidaId, id => new Partida(id));

        lock (partida)
        {
            if (partida.Ganador is not null || partida.Empate)
                throw new InvalidOperationException("La partida ya termino.");

            if (!string.Equals(jugador, partida.JugadorActual, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Es el turno de {partida.JugadorActual}.");

            if (partida.Tablero[request.Fila][request.Columna] != "")
                throw new InvalidOperationException("Esa casilla ya esta ocupada.");

            var simbolo = jugador.Equals("X", StringComparison.OrdinalIgnoreCase) ? "X" : "O";
            partida.Tablero[request.Fila][request.Columna] = simbolo;

            var movimiento = new Movimiento(
                partida.Movimientos.Count + 1,
                partida.Id,
                request.Fila,
                request.Columna,
                jugador,
                simbolo,
                DateTimeOffset.UtcNow);

            partida.Movimientos.Add(movimiento);

            if (HayGanador(partida.Tablero, simbolo))
                partida.Ganador = simbolo;
            else if (partida.Movimientos.Count == 9)
                partida.Empate = true;
            else
                partida.JugadorActual = simbolo == "X" ? "O" : "X";

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
        {
            return CrearRespuesta(partida);
        }
    }

    private static PartidaResponse CrearRespuesta(Partida partida)
    {
        var tablero = partida.Tablero
            .Select(fila => fila.ToArray())
            .ToArray();

        return new PartidaResponse(
            partida.Id,
            tablero,
            partida.JugadorActual,
            partida.Ganador,
            partida.Empate,
            partida.Movimientos.ToArray());
    }

    private static bool HayGanador(string[][] tablero, string simbolo)
    {
        for (var i = 0; i < 3; i++)
        {
            if (tablero[i].All(casilla => casilla == simbolo))
                return true;

            if (tablero.All(fila => fila[i] == simbolo))
                return true;
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
