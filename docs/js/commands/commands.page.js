import { API_BASE } from ".../core/config.js";
import { fetchJson } from ".../core/http.js";
import { escaparHtml, escaparJs } from ".../core/escape.js";
import { getServerId } from "../guilds/guilds.ui.js";

export function carregarComandos() {
    const content = document.getElementById("main-content");
    if (!content) return;

    if (!getServerId()) {
        content.innerHTML = `<h1>Comandos</h1><p>Nenhum servidor selecionado.</p>`;
        return;
    }

    content.innerHTML = `
        <h1>Comandos</h1>
        <div id="commands-list" class="dashboard"></div>
    `;

    carregarListaComandos();
}

export async function carregarListaComandos() {
    const container = document.getElementById("commands-list");
    if (!container) return;

    try {
        const data = await fetchJson(`${API_BASE}/api/commands/${getServerId()}`);
        container.innerHTML = "";

        if (!Array.isArray(data) || !data.length) {
            container.innerHTML = `
                <div class="card">
                    <h3>Sem comandos</h3>
                    <p>Nenhum comando encontrado para este servidor.</p>
                </div>
            `;
            return;
        }

        container.innerHTML = data.map(cmd => {
            const aliasesTexto = Array.isArray(cmd.aliases) ? cmd.aliases.join(", ") : "";
            const cooldown = Number(cmd.cooldownSeconds ?? 0);

            return `
                <div class="card command-card">
                    <div class="command-card-header">
                        <div>
                            <h3>${escaparHtml(cmd.commandName)}</h3>
                            <p>${escaparHtml(cmd.description || "Sem descrição.")}</p>
                        </div>
                    </div>

                    <div class="command-card-body">
                        <label class="command-toggle">
                            <input type="checkbox"
                                ${cmd.enabled ? "checked" : ""}
                                onchange="alterarStatusComando('${escaparJs(cmd.commandName)}', this.checked)">
                            Ativado
                        </label>

                        <div class="command-field">
                            <label for="aliases-${escaparHtml(cmd.commandName)}">Aliases</label>
                            <input
                                type="text"
                                id="aliases-${escaparHtml(cmd.commandName)}"
                                class="trigger-input"
                                value="${escaparHtml(aliasesTexto)}"
                                placeholder="Ex: r, dado, rolar">
                            <button
                                class="secondary-btn"
                                onclick="alterarAliasesComando('${escaparJs(cmd.commandName)}')">
                                Salvar aliases
                            </button>
                        </div>

                        <div class="command-field">
                            <label for="cooldown-${escaparHtml(cmd.commandName)}">Cooldown (segundos)</label>
                            <input
                                type="number"
                                id="cooldown-${escaparHtml(cmd.commandName)}"
                                class="trigger-input"
                                min="0"
                                value="${cooldown}">
                            <button
                                class="secondary-btn"
                                onclick="alterarCooldownComando('${escaparJs(cmd.commandName)}')">
                                Salvar cooldown
                            </button>
                        </div>
                    </div>
                </div>
            `;
        }).join("");
    } catch (err) {
        console.error("Erro ao carregar comandos:", err);
        container.innerHTML = `<div class="card"><p>Erro ao carregar comandos.</p></div>`;
    }
}

export function normalizarAliasesInput(valor) {
    return String(valor || "")
        .split(",")
        .map(x => x.trim())
        .filter(Boolean);
}

export async function alterarStatusComando(commandName, enabled) {
    try {
        await fetchJson(`${API_BASE}/api/commands/${getServerId()}/${encodeURIComponent(commandName)}/enabled`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(enabled)
        });

        await carregarListaComandos();
    } catch (err) {
        console.error("Erro ao alterar status do comando:", err);
        alert("Erro ao alterar status do comando.");
    }
}

export async function alterarAliasesComando(commandName) {
    const input = document.getElementById(`aliases-${commandName}`);
    if (!input) return;

    const aliases = normalizarAliasesInput(input.value);

    try {
        await fetchJson(`${API_BASE}/api/commands/${getServerId()}/${encodeURIComponent(commandName)}/aliases`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(aliases)
        });

        await carregarListaComandos();
    } catch (err) {
        console.error("Erro ao alterar aliases do comando:", err);
        alert("Erro ao alterar aliases do comando.");
    }
}

export async function alterarCooldownComando(commandName) {
    const input = document.getElementById(`cooldown-${commandName}`);
    if (!input) return;

    const cooldown = Number(input.value);

    if (Number.isNaN(cooldown) || cooldown < 0) {
        alert("Digite um cooldown válido.");
        return;
    }

    try {
        await fetchJson(`${API_BASE}/api/commands/${getServerId()}/${encodeURIComponent(commandName)}/cooldown`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(cooldown)
        });

        await carregarListaComandos();
    } catch (err) {
        console.error("Erro ao alterar cooldown do comando:", err);
        alert("Erro ao alterar cooldown do comando.");
    }
}