using Aplicaciones.Hubs;
using Aplicaciones.Services;
using Aplicaciones.Shared.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<PartidaService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origen =>
                Uri.TryCreate(origen, UriKind.Absolute, out var uri)
                && (uri.Port == 5002 || uri.Port == 7002))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("ClientPolicy");

app.MapPost("/api/Movimientos", async (
    MovimientoRequest request,
    PartidaService partidaService,
    IHubContext<PartidaHub> hubContext) =>
{
    try
    {
        var estado = partidaService.AgregarMovimiento(request);
        await hubContext.Clients.Group(estado.PartidaId)
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
