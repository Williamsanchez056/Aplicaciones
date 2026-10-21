namespace Aplicaciones.Shared.Contracts;

public sealed record PartidaResponse(
    string PartidaId,
    string[][] Tablero,
    string JugadorActual,
    string? Ganador,
    bool Empate,
    IReadOnlyList<Movimiento> Movimientos);
