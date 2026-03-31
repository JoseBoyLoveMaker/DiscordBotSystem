const LOCAL_API_BASE = "https://localhost:7229";
const PROD_API_BASE = "https://discordbotsystem-production.up.railway.app";

const API_BASE =
    window.location.hostname === "localhost" ||
    window.location.hostname === "127.0.0.1"
        ? LOCAL_API_BASE
        : PROD_API_BASE;

const BOT_CLIENT_ID = "1479279369477423135";
const BOT_PERMISSIONS = "8";

let currentUser = null;
let currentGuildId = null;

let deleteData = null;

// =========================
// TELAS
// =========================

function mostrarTela(id) {
    document.getElementById("loading-screen")?.classList.add("hidden");
    document.getElementById("auth-screen")?.classList.add("hidden");
    document.getElementById("app-screen")?.classList.add("hidden");

    document.getElementById(id)?.classList.remove("hidden");
}

// =========================
// UTIL
// =========================

function escaparHtml(texto) {
    return String(texto ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function escaparJs(texto) {
    return String(texto ?? "")
        .replaceAll("\\", "\\\\")
        .replaceAll("'", "\\'")
        .replaceAll('"', '\\"')
        .replaceAll("\n", "\\n")
        .replaceAll("\r", "\\r");
}

async function fetchJson(url, options = {}) {
    const finalOptions = {
        credentials: "include",
        ...options,
        headers: {
            ...(options.headers || {})
        }
    };

    const res = await fetch(url, finalOptions);

    if (!res.ok) {
        let body = "";
        try {
            body = await res.text();
        } catch {
            body = "";
        }

        throw new Error(`HTTP ${res.status} - ${body || "Erro na requisição"}`);
    }

    const contentType = res.headers.get("content-type") || "";
    if (contentType.includes("application/json")) {
        return await res.json();
    }

    return null;
}

// =========================
// AUTENTICAÇÃO
// =========================

async function verificarSessao() {
    try {
        const user = await fetchJson(`${API_BASE}/auth/me`);

        currentUser = user;
        preencherUsuario(user);

        await carregarServidores();

        mostrarTela("app-screen");
        carregarDashboard();
    } catch (err) {
        console.error("Erro ao verificar sessão:", err);
        mostrarTela("auth-screen");
    }
}

function iniciarLoginDiscord() {
    window.location.href = `${API_BASE}/auth/discord/login`;
}

async function logout() {
    try {
        await fetch(`${API_BASE}/auth/logout`, {
            method: "POST",
            credentials: "include"
        });
    } catch (err) {
        console.error("Erro ao sair:", err);
    }

    currentUser = null;
    currentGuildId = null;
    mostrarTela("auth-screen");
}

// =========================
// USUÁRIO
// =========================

function preencherUsuario(user) {
    const nameEl = document.getElementById("user-name");
    const avatarEl = document.getElementById("user-avatar");

    if (nameEl) {
        nameEl.textContent = user.globalName || user.username || "Usuário";
    }

    if (avatarEl && user.avatarUrl) {
        avatarEl.src = user.avatarUrl;
    }
}

// =========================
// SERVIDORES
// =========================

async function carregarServidores() {
    const strip = document.getElementById("servers-strip");
    if (!strip) return;

    try {
        const guilds = await fetchJson(`${API_BASE}/guilds`);
        strip.innerHTML = "";

        if (!guilds.length) {
            strip.innerHTML = `<div class="server-pill active">Nenhum servidor disponível</div>`;
            currentGuildId = null;
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
                currentGuildId = guild.id;
                atualizarServidorSelecionadoTexto(guild.name);
            }

            btn.addEventListener("click", () => {
                document.querySelectorAll(".server-pill").forEach(el => {
                    el.classList.remove("active");
                });

                btn.classList.add("active");
                currentGuildId = guild.id;
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

function getServerId() {
    return currentGuildId;
}

function atualizarServidorSelecionadoTexto(nome) {
    const el = document.getElementById("user-server");
    if (el) el.textContent = nome;
}

// =========================
// DASHBOARD
// =========================

async function loadStats() {
    const usersEl = document.getElementById("users");
    const xpEl = document.getElementById("xp");
    const commandsEl = document.getElementById("commands");
    const statusEl = document.getElementById("status");

    try {
        const data = await fetchJson(`${API_BASE}/api/bot/stats`);

        if (usersEl) usersEl.innerText = data?.users ?? 0;
        if (xpEl) xpEl.innerText = data?.totalXp ?? 0;
        if (commandsEl) commandsEl.innerText = data?.commands ?? 0;

        if (statusEl) {
            statusEl.innerText =
                data?.status === "Online" ? "🟢 Online" : "🔴 Offline";
        }
    } catch (err) {
        console.error("Erro ao carregar stats:", err);
        if (statusEl) {
            statusEl.innerText = "Erro ao conectar API";
        }
    }
}

function carregarDashboard() {
    const content = document.getElementById("main-content");
    if (!content) return;

    content.innerHTML = `
        <h1>Dashboard</h1>

        <div class="dashboard">
            <div class="card">
                <h3>👥 Usuários</h3>
                <p id="users">0</p>
            </div>

            <div class="card">
                <h3>✨ XP Total</h3>
                <p id="xp">0</p>
            </div>

            <div class="card">
                <h3>⚡ Comandos</h3>
                <p id="commands">0</p>
            </div>

            <div class="card status">
                <h3>📡 Status</h3>
                <p id="status">Carregando...</p>
            </div>
        </div>
    `;

    loadStats();
}

function adicionarBotAoServidor() {
    if (!currentGuildId) {
        alert("Selecione um servidor primeiro.");
        return;
    }

    const inviteUrl =
        `https://discord.com/oauth2/authorize` +
        `?client_id=${BOT_CLIENT_ID}` +
        `&permissions=${BOT_PERMISSIONS}` +
        `&scope=bot%20applications.commands` +
        `&guild_id=${currentGuildId}` +
        `&disable_guild_select=true`;

    window.open(inviteUrl, "_blank");
}

// =========================
// SERVIDOR / TRIGGERS
// =========================

function carregarServidor() {
    const content = document.getElementById("main-content");
    if (!content) return;

    if (!getServerId()) {
        content.innerHTML = `<h1>Servidor</h1><p>Nenhum servidor selecionado.</p>`;
        return;
    }

    content.innerHTML = `
        <h1>Responses</h1>

        <div class="server-page-actions">
            <div class="trigger-create-box">
                <input
                    type="text"
                    id="new-trigger-input"
                    class="trigger-input"
                    placeholder="Digite o nome da trigger">
                <button class="secondary-btn" onclick="criarTrigger()">Criar Trigger</button>
            </div>
        </div>

        <div id="triggers-list" class="dashboard"></div>

        <div id="delete-modal" class="modal hidden">
            <div class="modal-content">
                <h3 id="delete-modal-title">Excluir</h3>
                <p id="delete-modal-message">Tem certeza?</p>

                <div id="delete-preview" class="delete-preview"></div>

                <div class="modal-buttons">
                    <button class="btn-cancel" onclick="fecharModalExcluir()">Cancelar</button>
                    <button class="btn-delete" onclick="confirmarExclusao()">Excluir</button>
                </div>
            </div>
        </div>
    `;

    carregarListaTriggers();
}

async function carregarListaTriggers() {
    const container = document.getElementById("triggers-list");
    if (!container) return;

    if (!getServerId()) {
        container.innerHTML = `<p>Nenhum servidor selecionado.</p>`;
        return;
    }

    try {
        const data = await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses`);
        container.innerHTML = "";

        if (!data.length) {
            container.innerHTML = `
                <div class="card">
                    <h3>Sem triggers</h3>
                    <p class="empty-text">Crie a primeira trigger para começar.</p>
                </div>
            `;
            return;
        }

        container.innerHTML = data.map(item => criarCardTrigger(item)).join("");
    } catch (err) {
        console.error("Erro ao carregar triggers:", err);
        container.innerHTML = `<div class="card"><p>Erro ao carregar triggers.</p></div>`;
    }
}

function criarCardTrigger(item) {
    const responses = Array.isArray(item.responses) ? item.responses : [];
    const totalResponses = responses.length;

    return `
        <div class="card trigger-card trigger-list-card">
            <button
                class="trigger-delete-btn"
                title="Excluir trigger"
                onclick="abrirModalExcluirTrigger('${escaparJs(item.trigger)}')">
                🗑️
            </button>

            <button
                type="button"
                class="trigger-open-btn"
                onclick="abrirDetalhesTrigger('${escaparJs(item.trigger)}')">
                <div class="trigger-header">
                    <div class="trigger-header-main">
                        <h3 class="trigger-title">${escaparHtml(item.trigger)}</h3>
                        <p class="trigger-count">${totalResponses} response${totalResponses === 1 ? "" : "s"}</p>
                    </div>
                </div>
            </button>
        </div>
    `;
}

async function abrirDetalhesTrigger(trigger) {
    const content = document.getElementById("main-content");
    if (!content || !getServerId()) return;

    content.innerHTML = `
        <div class="trigger-details-top">
            <button class="back-btn" onclick="carregarServidor()">← Voltar</button>
        </div>

        <div id="trigger-details-content" class="trigger-details-content">
            <div class="card">
                <p>Carregando responses...</p>
            </div>
        </div>

        <div id="delete-modal" class="modal hidden">
            <div class="modal-content">
                <h3 id="delete-modal-title">Excluir</h3>
                <p id="delete-modal-message">Tem certeza?</p>

                <div id="delete-preview" class="delete-preview"></div>

                <div class="modal-buttons">
                    <button class="btn-cancel" onclick="fecharModalExcluir()">Cancelar</button>
                    <button class="btn-delete" onclick="confirmarExclusao()">Excluir</button>
                </div>
            </div>
        </div>
    `;

    try {
        const data = await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses`);
        const item = data.find(x => String(x.trigger) === String(trigger));

        const container = document.getElementById("trigger-details-content");
        if (!container) return;

        if (!item) {
            container.innerHTML = `
                <div class="card">
                    <h3>Trigger não encontrada</h3>
                </div>
            `;
            return;
        }

        container.innerHTML = criarDetalhesTrigger(item);
        configurarAutoSaveResponses(item.trigger, item.responses || []);
    } catch (err) {
        console.error("Erro ao abrir detalhes da trigger:", err);
        const container = document.getElementById("trigger-details-content");
        if (container) {
            container.innerHTML = `
                <div class="card">
                    <p>Erro ao carregar responses da trigger.</p>
                </div>
            `;
        }
    }
}

function criarDetalhesTrigger(item) {
    const responses = Array.isArray(item.responses) ? item.responses : [];
    const totalResponses = responses.length;

    const respostasHtml = responses.length
        ? responses.map((resp, index) => `
            <div class="response-row response-row-details">
                <textarea
                    id="response-${escaparHtml(item.trigger)}-${index}"
                    class="edit-box response-edit-box"
                    rows="3"
                    data-original="${escaparHtml(resp)}">${escaparHtml(resp)}</textarea>

                <div class="response-actions">
                    <button
                        class="mini-delete-btn"
                        title="Excluir resposta"
                        onclick="abrirModalExcluirResponse('${escaparJs(item.trigger)}', ${index}, '${escaparJs(resp)}')">
                        🗑️
                    </button>
                </div>
            </div>
        `).join("")
        : `<p class="empty-text">Nenhuma response cadastrada para esta trigger.</p>`;

    return `
        <div class="card trigger-details-card">
            <button
                class="trigger-delete-btn"
                title="Excluir trigger"
                onclick="abrirModalExcluirTrigger('${escaparJs(item.trigger)}')">
                🗑️
            </button>

            <div class="trigger-header">
                <div class="trigger-header-main">
                    <h1 class="trigger-details-title">${escaparHtml(item.trigger)}</h1>
                    <p class="trigger-count">${totalResponses} response${totalResponses === 1 ? "" : "s"}</p>
                </div>
            </div>

            <div class="trigger-responses trigger-responses-details">
                ${respostasHtml}
            </div>
        </div>
    `;
}

function configurarAutoSaveResponses(trigger, responses) {
    responses.forEach((resp, index) => {
        const textarea = document.getElementById(`response-${trigger}-${index}`);
        if (!textarea) return;

        textarea.addEventListener("blur", async () => {
            const valorAtual = textarea.value.trim();
            const valorOriginal = (textarea.dataset.original || "").trim();

            if (!valorAtual || valorAtual === valorOriginal) {
                textarea.value = valorOriginal;
                return;
            }

            textarea.disabled = true;

            try {
                await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses/${encodeURIComponent(trigger)}/${index}`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(valorAtual)
                });

                textarea.dataset.original = valorAtual;
            } catch (err) {
                console.error("Erro ao editar response:", err);
                textarea.value = valorOriginal;
                alert("Erro ao salvar a response.");
            } finally {
                textarea.disabled = false;
            }
        });
    });
}

async function criarTrigger() {
    const input = document.getElementById("new-trigger-input");
    if (!input) return;

    const trigger = input.value.trim();

    if (!trigger) {
        alert("Digite uma trigger válida.");
        return;
    }

    try {
        await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(trigger)
        });

        input.value = "";
        await carregarListaTriggers();
    } catch (err) {
        console.error("Erro ao criar trigger:", err);
        alert("Erro ao criar trigger.");
    }
}

async function editarResponse(trigger, index) {
    const textarea = document.getElementById(`response-${trigger}-${index}`);
    if (!textarea) return;

    const nova = textarea.value.trim();

    if (!nova) {
        alert("Digite uma resposta válida.");
        return;
    }

    try {
        await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses/${encodeURIComponent(trigger)}/${index}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(nova)
        });
    } catch (err) {
        console.error("Erro ao editar response:", err);
        alert("Erro ao editar response.");
    }
}

// =========================
// EXCLUSÃO
// =========================

function abrirModalExcluirTrigger(trigger) {
    deleteData = {
        type: "trigger",
        trigger
    };

    const modal = document.getElementById("delete-modal");
    const title = document.getElementById("delete-modal-title");
    const message = document.getElementById("delete-modal-message");
    const preview = document.getElementById("delete-preview");

    if (title) title.textContent = "Excluir trigger";
    if (message) message.textContent = "Tem certeza que deseja excluir esta trigger inteira?";

    if (preview) {
        preview.innerHTML = `
            <strong>Trigger:</strong><br>
            ${escaparHtml(trigger)}
        `;
    }

    modal?.classList.remove("hidden");
}

function abrirModalExcluirResponse(trigger, index, responseTexto) {
    deleteData = {
        type: "response",
        trigger,
        index
    };

    const modal = document.getElementById("delete-modal");
    const title = document.getElementById("delete-modal-title");
    const message = document.getElementById("delete-modal-message");
    const preview = document.getElementById("delete-preview");

    if (title) title.textContent = "Excluir resposta";
    if (message) message.textContent = "Tem certeza que deseja excluir esta resposta?";

    if (preview) {
        preview.textContent = responseTexto ?? "";
    }

    modal?.classList.remove("hidden");
}

function fecharModalExcluir() {
    deleteData = null;
    document.getElementById("delete-modal")?.classList.add("hidden");
}

async function confirmarExclusao() {
    if (!deleteData || !currentGuildId) {
        fecharModalExcluir();
        return;
    }

    try {
        if (deleteData.type === "trigger") {
            await fetchJson(
                `${API_BASE}/api/guilds/${currentGuildId}/responses/${encodeURIComponent(deleteData.trigger)}`,
                {
                    method: "DELETE"
                }
            );

            carregarServidor();
            return;
        }

        if (deleteData.type === "response") {
            await fetchJson(
                `${API_BASE}/api/guilds/${currentGuildId}/responses/${encodeURIComponent(deleteData.trigger)}/${deleteData.index}`,
                {
                    method: "DELETE"
                }
            );

            await abrirDetalhesTrigger(deleteData.trigger);
            return;
        }
    } catch (err) {
        console.error("Erro ao excluir:", err);
        alert("Erro ao excluir.");
    } finally {
        fecharModalExcluir();
    }
}

// =========================
// COMANDOS
// =========================

function carregarComandos() {
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

async function carregarListaComandos() {
    const container = document.getElementById("commands-list");
    if (!container) return;

    try {
        const data = await fetchJson(`${API_BASE}/api/commands/${getServerId()}`);
        container.innerHTML = "";

        data.forEach(cmd => {
            container.innerHTML += `
                <div class="card">
                    <h3>${escaparHtml(cmd.commandName)}</h3>

                    <label>
                        <input type="checkbox"
                            ${cmd.enabled ? "checked" : ""}
                            onchange="alterarStatusComando('${escaparJs(cmd.commandName)}', this.checked)">
                        Ativado
                    </label>
                </div>
            `;
        });
    } catch (err) {
        console.error("Erro ao carregar comandos:", err);
        container.innerHTML = "Erro ao carregar comandos";
    }
}

async function alterarStatusComando(commandName, enabled) {
    try {
        await fetchJson(`${API_BASE}/api/commands/${getServerId()}/${encodeURIComponent(commandName)}/enabled`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(enabled)
        });
    } catch (err) {
        console.error("Erro ao alterar status do comando:", err);
        alert("Erro ao alterar status do comando.");
    }
}

// =========================
// MENU
// =========================

function fecharMenu() {
    document.querySelector(".sidebar")?.classList.remove("closed");
}

// =========================
// INICIALIZAÇÃO
// =========================

document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.getElementById("menu-toggle");
    const sidebar = document.querySelector(".sidebar");

    const loginBtn = document.getElementById("login-btn");
    const logoutBtn = document.getElementById("logout-btn");
    const inviteBotBtn = document.getElementById("invite-bot-btn");

    const btnDashboard = document.getElementById("btn-dashboard");
    const btnServidor = document.getElementById("btn-servidor");
    const btnComandos = document.getElementById("btn-comandos");

    if (loginBtn) loginBtn.addEventListener("click", iniciarLoginDiscord);
    if (logoutBtn) logoutBtn.addEventListener("click", logout);
    if (inviteBotBtn) inviteBotBtn.addEventListener("click", adicionarBotAoServidor);

    if (btnDashboard) {
        btnDashboard.onclick = (e) => {
            e.preventDefault();
            carregarDashboard();
        };
    }

    if (btnServidor) {
        btnServidor.onclick = (e) => {
            e.preventDefault();
            carregarServidor();
        };
    }

    if (btnComandos) {
        btnComandos.onclick = (e) => {
            e.preventDefault();
            carregarComandos();
        };
    }

    if (toggle && sidebar) {
        toggle.onclick = () => sidebar.classList.toggle("closed");
    }

    console.log("API_BASE atual:", API_BASE);

    mostrarTela("loading-screen");
    verificarSessao();

    setInterval(() => {
        if (!document.getElementById("app-screen")?.classList.contains("hidden")) {
            loadStats();
        }
    }, 5000);
});