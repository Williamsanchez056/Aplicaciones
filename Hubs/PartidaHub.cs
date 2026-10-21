using Microsoft.AspNetCore.SignalR;

namespace Aplicaciones.Hubs;

public sealed class PartidaHub : Hub
{
    public async Task UnirsePartida(string partidaId)
    {
        if (string.IsNullOrWhiteSpace(partidaId))
            throw new HubException("El identificador de la partida es obligatorio.");

        await Groups.AddToGroupAsync(Context.ConnectionId, partidaId.Trim());
    }
}
