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

// La raiz del servidor no es el tablero; muestra como abrir el cliente.
app.MapGet("/", () => Results.Content("""
    <!doctype html>
    <html lang="es">
    <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>TicTacToe - Servidor</title>
        <style>
            body { font-family: system-ui, sans-serif; max-width: 680px; margin: 12vh auto; padding: 0 24px; line-height: 1.6; }
            code { background: #f1f1f1; padding: 3px 6px; border-radius: 4px; }
            a { display: inline-block; margin: 8px 0 16px; }
        </style>
    </head>
    <body>
        <h1>Servidor de TicTacToe activo</h1>
        <p>El servidor de la API y SignalR esta funcionando. El tablero se abre desde el proyecto <strong>Aplicaciones.Client</strong>.</p>
        <p>Inicia el cliente en otra terminal desde la carpeta raiz del repositorio:</p>
        <p><code>dotnet run --project Aplicaciones.Client --urls http://localhost:5002</code></p>
        <p>Luego abre <a href="http://localhost:5002">http://localhost:5002</a>.</p>
        <p>La API esta disponible en este mismo servidor en <code>/api/Movimientos</code> y SignalR en <code>/partidaHub</code>.</p>
    </body>
    </html>
    """, "text/html; charset=utf-8"));

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
