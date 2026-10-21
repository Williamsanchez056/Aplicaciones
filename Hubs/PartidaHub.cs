using Microsoft.AspNetCore.SignalR;

namespace Aplicaciones.Hubs;

public sealed class PartidaHub : Hub
{
    public async Task UnirsePartida(string partidaId)
    {
        if (string.IsNullOrWhiteSpace(partidaId) || partidaId.Trim().Length > 50)
            throw new HubException("El identificador de la partida no es valido.");

        await Groups.AddToGroupAsync(Context.ConnectionId, partidaId.Trim());
    }
}
