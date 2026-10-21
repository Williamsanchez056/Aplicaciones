using Aplicaciones.Hubs;
using Aplicaciones.Models;
using Aplicaciones.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<PartidaService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/Movimientos", async (
    MovimientoRequest request,
    PartidaService partidaService,
    IHubContext<PartidaHub> hubContext) =>
{
    try
    {
        var estado = partidaService.AgregarMovimiento(request);
        await hubContext.Clients.Group(request.PartidaId.Trim())
            .SendAsync("MovimientoActualizado", estado);
        return Results.Ok(estado);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapGet("/api/Movimientos/{partidaId}", (
    string partidaId,
    PartidaService partidaService) =>
{
    try
    {
        return Results.Ok(partidaService.ObtenerPartida(partidaId));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.MapHub<PartidaHub>("/partidaHub");

app.Run();
