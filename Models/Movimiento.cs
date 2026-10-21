namespace Aplicaciones.Models;

public sealed record MovimientoRequest(
    string PartidaId,
    int Fila,
    int Columna,
    string Jugador);

public sealed record Movimiento(
    int Numero,
    string PartidaId,
    int Fila,
    int Columna,
    string Jugador,
    string Simbolo,
    DateTimeOffset Fecha);

public sealed record PartidaResponse(
    string PartidaId,
    string[][] Tablero,
    string JugadorActual,
    string? Ganador,
    bool Empate,
    IReadOnlyList<Movimiento> Movimientos);
