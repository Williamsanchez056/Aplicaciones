"use strict";

const partidaInput = document.getElementById("partidaId");
const jugadorInput = document.getElementById("jugador");
const estado = document.getElementById("estado");
const historial = document.getElementById("historial");
const tableroElemento = document.getElementById("tablero");
let conexion = null;
let partidaIdActual = "";

const apiBase = window.location.protocol + "//" + window.location.hostname + ":5001";

for (let fila = 0; fila < 3; fila++) {
    for (let columna = 0; columna < 3; columna++) {
        const boton = document.createElement("button");
        boton.className = "casilla";
        boton.type = "button";
        boton.setAttribute("aria-label", "Fila " + (fila + 1) + ", columna " + (columna + 1));
        boton.addEventListener("click", () => hacerMovimiento(fila, columna));
        tableroElemento.appendChild(boton);
    }
}

document.getElementById("conectar").addEventListener("click", conectar);
document.getElementById("nuevaPartida").addEventListener("click", async () => {
    if (conexion) await conexion.stop();
    conexion = null;
    const sufijo = Math.random().toString(36).slice(2, 8);
    partidaInput.value = "partida-" + Date.now().toString(36) + "-" + sufijo;
    limpiarTablero();
    estado.textContent = "Nuevo identificador generado. Comparte el identificador y pulsa Conectar.";
});

async function conectar() {
    const partidaId = partidaInput.value.trim();
    if (!partidaId || partidaId.length > 50) {
        estado.textContent = "Escribe un identificador valido.";
        return;
    }

    try {
        if (conexion) await conexion.stop();
        partidaIdActual = partidaId;
        conexion = new signalR.HubConnectionBuilder()
            .withUrl(apiBase + "/partidaHub")
            .withAutomaticReconnect()
            .build();

        conexion.on("MovimientoActualizado", pintarPartida);
        conexion.onreconnecting(() => {
            estado.textContent = "Conexion perdida. Reconectando...";
            bloquearTablero(true);
        });
        conexion.onreconnected(async () => {
            try {
                await conexion.invoke("UnirsePartida", partidaIdActual);
                await cargarPartida();
            } catch (error) { mostrarError(error); }
        });
        conexion.onclose(() => {
            estado.textContent = "Conexion cerrada. Pulsa Conectar para volver a intentarlo.";
            bloquearTablero(true);
        });

        await conexion.start();
        await conexion.invoke("UnirsePartida", partidaIdActual);
        await cargarPartida();
        estado.textContent = "Conectado a " + partidaIdActual + ". Eres " + jugadorInput.value + ".";
    } catch (error) { mostrarError(error); }
}

async function cargarPartida() {
    const respuesta = await fetch(apiBase + "/api/Movimientos/" + encodeURIComponent(partidaIdActual));
    if (respuesta.status === 404) {
        limpiarTablero();
        bloquearTablero(false);
        estado.textContent = "Partida lista. Empieza el jugador X.";
        return;
    }
    if (!respuesta.ok) throw new Error("No se pudo consultar la partida.");
    pintarPartida(await respuesta.json());
}

async function hacerMovimiento(fila, columna) {
    if (!conexion || conexion.state !== signalR.HubConnectionState.Connected) {
        estado.textContent = "Primero conecta con la partida.";
        return;
    }

    try {
        const respuesta = await fetch(apiBase + "/api/Movimientos", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                partidaId: partidaIdActual,
                fila,
                columna,
                jugador: jugadorInput.value
            })
        });
        const resultado = await respuesta.json();
        if (!respuesta.ok) {
            estado.textContent = resultado.error || "No se pudo registrar el movimiento.";
            return;
        }
        pintarPartida(resultado);
    } catch (error) { mostrarError(error); }
}

function pintarPartida(partida) {
    const botones = tableroElemento.querySelectorAll(".casilla");
    partida.tablero.forEach((fila, i) => fila.forEach((valor, j) => {
        botones[i * 3 + j].textContent = valor;
        botones[i * 3 + j].disabled = Boolean(valor) || Boolean(partida.ganador) || partida.empate;
    }));

    historial.replaceChildren();
    partida.movimientos.forEach(movimiento => {
        const item = document.createElement("li");
        item.textContent = movimiento.simbolo + " jugo en fila " + (movimiento.fila + 1)
            + ", columna " + (movimiento.columna + 1);
        historial.appendChild(item);
    });

    if (partida.ganador) estado.textContent = "Gano " + partida.ganador + ".";
    else if (partida.empate) estado.textContent = "La partida termino en empate.";
    else estado.textContent = "Turno de " + partida.jugadorActual + ".";
}

function limpiarTablero() {
    tableroElemento.querySelectorAll(".casilla").forEach(boton => {
        boton.textContent = "";
        boton.disabled = true;
    });
    historial.replaceChildren();
}

function bloquearTablero(bloqueado) {
    tableroElemento.querySelectorAll(".casilla").forEach(boton => {
        boton.disabled = bloqueado;
    });
}

function mostrarError(error) {
    estado.textContent = "Error: " + (error?.message || "no se pudo completar la operacion.");
    console.error(error);
}

limpiarTablero();
