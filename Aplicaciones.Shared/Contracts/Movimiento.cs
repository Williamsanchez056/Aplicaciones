namespace Aplicaciones.Shared.Contracts;

public sealed record Movimiento(
    int Numero,
    string PartidaId,
    int Fila,
    int Columna,
    string Jugador,
    string Simbolo,
    DateTimeOffset Fecha);
