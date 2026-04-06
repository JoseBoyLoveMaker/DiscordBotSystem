import { API_BASE } from ".../core/config.js";
import { appState } from ".../core/state.js";
import { fetchJson } from ".../core/http.js";
import { escaparHtml } from ".../core/escape.js";
import { carregarDashboard } from "../dashboard/dashboard.page.js";

export function getServerId() {
    return appState.currentGuildId;
}

export function atualizarServidorSelecionadoTexto(nome) {
    const el = document.getElementById("user-server");
    if (el) el.textContent = nome;
}

export async function carregarServidores() {
    const strip = document.getElementById("servers-strip");
    if (!strip) return;

    try {
        const guilds = await fetchJson(`${API_BASE}/guilds`);
        strip.innerHTML = "";

        if (!guilds.length) {
            strip.innerHTML = `<div class="server-pill active">Nenhum servidor disponível</div>`;
            appState.currentGuildId = null;
            atualizarServidorSelecionadoTexto("Nenhum servidor selecionado");
            return;
        }

        guilds.forEach((guild, index) => {
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "server-pill";
            btn.dataset.guildId = guild.id;
            btn.dataset.guildName = guild.name;

            const iconUrl = guild.icon
                ? `https://cdn.discordapp.com/icons/${guild.id}/${guild.icon}.png?size=64`
                : "";

            btn.innerHTML = `
                <span class="server-pill-inner">
                    ${
                        iconUrl
                            ? `<img class="server-pill-avatar" src="${iconUrl}" alt="${escaparHtml(guild.name)}">`
                            : `<span class="server-pill-avatar server-pill-fallback">${escaparHtml(guild.name.charAt(0).toUpperCase())}</span>`
                    }
                    <span class="server-pill-name">${escaparHtml(guild.name)}</span>
                </span>
            `;

            if (index === 0) {
                btn.classList.add("active");
                appState.currentGuildId = guild.id;
                atualizarServidorSelecionadoTexto(guild.name);
            }

            btn.addEventListener("click", () => {
                document.querySelectorAll(".server-pill").forEach(el => {
                    el.classList.remove("active");
                });

                btn.classList.add("active");
                appState.currentGuildId = guild.id;
                atualizarServidorSelecionadoTexto(guild.name);

                carregarDashboard();
            });

            strip.appendChild(btn);
        });
    } catch (err) {
        console.error("Erro ao carregar servidores:", err);
        strip.innerHTML = `<div class="server-pill active">Erro ao carregar servidores</div>`;
    }
}

export async function carregarCanaisGuild() {
    return await fetchJson(`${API_BASE}/guilds/${getServerId()}/channels`);
}

export async function carregarCargosGuild() {
    return await fetchJson(`${API_BASE}/guilds/${getServerId()}/roles`);
}