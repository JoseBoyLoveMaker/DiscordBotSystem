import { API_BASE } from ".../core/config.js";
import { fetchJson } from ".../core/http.js";
import { escaparHtml } from ".../core/escape.js";
import { getServerId, carregarCanaisGuild, carregarCargosGuild } from "../guilds/guilds.ui.js";

export function carregarModeracao() {
    const content = document.getElementById("main-content");
    if (!content) return;

    if (!getServerId()) {
        content.innerHTML = `<h1>Moderação</h1><p>Nenhum servidor selecionado.</p>`;
        return;
    }

    content.innerHTML = `
        <h1>Moderação</h1>

        <div class="moderation-page">
            <div class="moderation-tabs">
                <button class="moderation-tab active" id="mod-tab-welcome" onclick="abrirAbaModeracao('welcome')">
                    Entrada / Saída
                </button>

                <button class="moderation-tab" id="mod-tab-roles" onclick="abrirAbaModeracao('roles')">
                    Cargos
                </button>
            </div>

            <div id="moderation-content" class="moderation-content">
                <div class="card">
                    <p>Carregando configurações...</p>
                </div>
            </div>
        </div>
    `;

    abrirAbaModeracao("welcome");
}

export function definirAbaModeracaoAtiva(tab) {
    document.getElementById("mod-tab-welcome")?.classList.remove("active");
    document.getElementById("mod-tab-roles")?.classList.remove("active");

    if (tab === "welcome") {
        document.getElementById("mod-tab-welcome")?.classList.add("active");
    }

    if (tab === "roles") {
        document.getElementById("mod-tab-roles")?.classList.add("active");
    }
}

export async function abrirAbaModeracao(tab) {
    definirAbaModeracaoAtiva(tab);

    if (tab === "welcome") {
        await carregarModeracaoWelcomeLeave();
        return;
    }

    if (tab === "roles") {
        await carregarModeracaoRoles();
    }
}

export function criarOptionsSelect(items, selectedValue, placeholder) {
    const baseOption = `<option value="">${escaparHtml(placeholder)}</option>`;

    const options = (items || []).map(item => `
        <option value="${escaparHtml(item.id)}" ${String(item.id) === String(selectedValue ?? "") ? "selected" : ""}>
            ${escaparHtml(item.name)}
        </option>
    `).join("");

    return baseOption + options;
}

export async function carregarModeracaoWelcomeLeave() {
    const container = document.getElementById("moderation-content");
    if (!container || !getServerId()) return;

    container.innerHTML = `
        <div class="card">
            <p>Carregando Entrada / Saída...</p>
        </div>
    `;

    try {
        const [config, channels, roles] = await Promise.all([
            fetchJson(`${API_BASE}/api/guilds/${getServerId()}/moderation`),
            carregarCanaisGuild(),
            carregarCargosGuild()
        ]);

        const welcome = config?.welcomeConfig ?? {};
        const leave = config?.leaveConfig ?? {};
        const role = config?.roleConfig ?? {};

        container.innerHTML = `
            <div class="moderation-grid">
                <div class="card moderation-card">
                    <h3>📥 Mensagem de Entrada</h3>

                    <label class="switch-row">
                        <input type="checkbox" id="welcome-enabled" ${welcome.enabled ? "checked" : ""}>
                        <span>Ativar mensagem de entrada</span>
                    </label>

                    <div class="form-group">
                        <label for="welcome-channel">Canal</label>
                        <select id="welcome-channel" class="trigger-input">
                            ${criarOptionsSelect(channels, welcome.channelId, "Selecione um canal")}
                        </select>
                    </div>

                    <div class="form-group">
                        <label for="welcome-message">Mensagem</label>
                        <textarea id="welcome-message" class="edit-box moderation-textarea" rows="5" placeholder="Digite a mensagem de entrada">${escaparHtml(welcome.message || "")}</textarea>
                    </div>

                    <div class="form-group">
                        <label for="autorole-id">Cargo automático ao entrar</label>
                        <select id="autorole-id" class="trigger-input">
                            ${criarOptionsSelect(roles, role.autoRoleId, "Nenhum cargo")}
                        </select>
                    </div>

                    <div class="form-actions">
                        <button class="secondary-btn" onclick="salvarWelcomeLeave()">Salvar Entrada / Saída</button>
                    </div>
                </div>

                <div class="card moderation-card">
                    <h3>📤 Mensagem de Saída</h3>

                    <label class="switch-row">
                        <input type="checkbox" id="leave-enabled" ${leave.enabled ? "checked" : ""}>
                        <span>Ativar mensagem de saída</span>
                    </label>

                    <div class="form-group">
                        <label for="leave-channel">Canal</label>
                        <select id="leave-channel" class="trigger-input">
                            ${criarOptionsSelect(channels, leave.channelId, "Selecione um canal")}
                        </select>
                    </div>

                    <div class="form-group">
                        <label for="leave-message">Mensagem</label>
                        <textarea id="leave-message" class="edit-box moderation-textarea" rows="5" placeholder="Digite a mensagem de saída">${escaparHtml(leave.message || "")}</textarea>
                    </div>

                    <div class="moderation-help">
                        Variáveis recomendadas: <code>{user}</code>, <code>{username}</code>, <code>{server}</code>
                    </div>
                </div>
            </div>
        `;
    } catch (err) {
        console.error("Erro ao carregar moderação Entrada/Saída:", err);
        container.innerHTML = `<div class="card"><p>Erro ao carregar configurações de Entrada / Saída.</p></div>`;
    }
}

export async function salvarWelcomeLeave() {
    if (!getServerId()) return;

    const payload = {
        welcomeConfig: {
            enabled: document.getElementById("welcome-enabled")?.checked ?? false,
            channelId: document.getElementById("welcome-channel")?.value || null,
            message: document.getElementById("welcome-message")?.value?.trim() || ""
        },
        leaveConfig: {
            enabled: document.getElementById("leave-enabled")?.checked ?? false,
            channelId: document.getElementById("leave-channel")?.value || null,
            message: document.getElementById("leave-message")?.value?.trim() || ""
        },
        roleConfig: {
            autoRoleId: document.getElementById("autorole-id")?.value || null,
            levelRoleRewards: []
        }
    };

    try {
        await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/moderation`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        alert("Configurações de entrada e saída salvas com sucesso.");
        await carregarModeracaoWelcomeLeave();
    } catch (err) {
        console.error("Erro ao salvar Entrada / Saída:", err);
        alert("Erro ao salvar configurações de entrada e saída.");
    }
}

export async function carregarModeracaoRoles() {
    const container = document.getElementById("moderation-content");
    if (!container || !getServerId()) return;

    container.innerHTML = `
        <div class="card">
            <p>Carregando cargos...</p>
        </div>
    `;

    try {
        const [config, roles] = await Promise.all([
            fetchJson(`${API_BASE}/api/guilds/${getServerId()}/moderation`),
            carregarCargosGuild()
        ]);

        const roleConfig = config?.roleConfig ?? {};
        const rewards = Array.isArray(roleConfig.levelRoleRewards)
            ? roleConfig.levelRoleRewards
            : [];

        container.innerHTML = `
            <div class="card moderation-card">
                <div class="moderation-header-row">
                    <div>
                        <h3>⭐ Cargos por nível</h3>
                        <p class="moderation-subtext">
                            Configure cargos com requisitos separados de nível de chat e nível de call.
                        </p>
                    </div>

                    <button class="secondary-btn" onclick='adicionarRegraCargo()'>
                        Adicionar regra
                    </button>
                </div>

                <div id="level-roles-list" class="level-roles-list">
                    ${
                        rewards.length
                            ? rewards.map((reward, index) => criarLinhaRegraCargo(reward, index, roles)).join("")
                            : `<p class="empty-text">Nenhuma regra cadastrada.</p>`
                    }
                </div>

                <div class="form-actions">
                    <button class="secondary-btn" onclick="salvarRoleRewards()">Salvar cargos</button>
                </div>
            </div>
        `;
    } catch (err) {
        console.error("Erro ao carregar cargos da moderação:", err);
        container.innerHTML = `<div class="card"><p>Erro ao carregar cargos.</p></div>`;
    }
}

export function criarLinhaRegraCargo(reward = {}, index = 0, roles = []) {
    return `
        <div class="level-role-row" data-index="${index}">
            <div class="form-group">
                <label>Cargo</label>
                <select class="trigger-input level-role-id">
                    ${criarOptionsSelect(roles, reward.roleId, "Selecione um cargo")}
                </select>
            </div>

            <div class="form-group">
                <label>Nível mínimo chat</label>
                <input type="number" class="trigger-input level-chat" min="0" value="${Number(reward.minChatLevel ?? 0)}">
            </div>

            <div class="form-group">
                <label>Nível mínimo call</label>
                <input type="number" class="trigger-input level-call" min="0" value="${Number(reward.minCallLevel ?? 0)}">
            </div>

            <div class="form-group role-row-remove-group">
                <label>&nbsp;</label>
                <button class="logout-btn" type="button" onclick="removerRegraCargo(this)">Remover</button>
            </div>
        </div>
    `;
}

export async function adicionarRegraCargo() {
    const list = document.getElementById("level-roles-list");
    if (!list) return;

    try {
        const roles = await carregarCargosGuild();

        const emptyText = list.querySelector(".empty-text");
        if (emptyText) {
            list.innerHTML = "";
        }

        const index = list.querySelectorAll(".level-role-row").length;
        list.insertAdjacentHTML("beforeend", criarLinhaRegraCargo({}, index, roles));
    } catch (err) {
        console.error("Erro ao adicionar regra de cargo:", err);
        alert("Erro ao carregar cargos do servidor.");
    }
}

export function removerRegraCargo(button) {
    const row = button.closest(".level-role-row");
    row?.remove();

    const list = document.getElementById("level-roles-list");
    if (list && !list.querySelector(".level-role-row")) {
        list.innerHTML = `<p class="empty-text">Nenhuma regra cadastrada.</p>`;
    }
}

export async function salvarRoleRewards() {
    if (!getServerId()) return;

    const rows = Array.from(document.querySelectorAll(".level-role-row"));

    const rewards = rows.map(row => ({
        roleId: row.querySelector(".level-role-id")?.value || null,
        minChatLevel: Number(row.querySelector(".level-chat")?.value || 0),
        minCallLevel: Number(row.querySelector(".level-call")?.value || 0)
    })).filter(x => x.roleId);

    const payload = {
        welcomeConfig: {
            enabled: false,
            channelId: null,
            message: ""
        },
        leaveConfig: {
            enabled: false,
            channelId: null,
            message: ""
        },
        roleConfig: {
            autoRoleId: null,
            levelRoleRewards: rewards
        }
    };

    try {
        const atual = await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/moderation`);

        payload.welcomeConfig = atual?.welcomeConfig ?? payload.welcomeConfig;
        payload.leaveConfig = atual?.leaveConfig ?? payload.leaveConfig;
        payload.roleConfig.autoRoleId = atual?.roleConfig?.autoRoleId ?? null;

        await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/moderation`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        alert("Cargos salvos com sucesso.");
        await carregarModeracaoRoles();
    } catch (err) {
        console.error("Erro ao salvar cargos:", err);
        alert("Erro ao salvar cargos.");
    }
}