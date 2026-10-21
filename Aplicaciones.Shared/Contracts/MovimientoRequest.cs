namespace Aplicaciones.Shared.Contracts;

public sealed record MovimientoRequest(
    string PartidaId,
    int Fila,
    int Columna,
    string Jugador);
