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
    const controller = new AbortController();
    const timeoutMs = options.timeoutMs ?? 8000;

    const timeoutId = setTimeout(() => {
        controller.abort();
    }, timeoutMs);

    try {
        const finalOptions = {
            credentials: "include",
            ...options,
            signal: controller.signal,
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
    } catch (err) {
        if (err.name === "AbortError") {
            throw new Error(`Timeout ao acessar ${url}`);
        }

        throw err;
    } finally {
        clearTimeout(timeoutId);
    }
}

// =========================
// AUTENTICAÇÃO
// =========================

async function verificarSessao() {
    try {
        console.log("[AUTH] Verificando sessão...");

        const user = await fetchJson(`${API_BASE}/auth/me`, { timeoutMs: 7000 });

        console.log("[AUTH] Sessão encontrada:", user);

        currentUser = user;
        preencherUsuario(user);

        await carregarServidores();

        mostrarTela("app-screen");
        carregarDashboard();
    } catch (err) {
        console.error("[AUTH] Sessão não encontrada ou erro:", err);
        mostrarTela("auth-screen");
    }
}

let loginIniciado = false;

function iniciarLoginDiscord() {
    if (loginIniciado) return;

    loginIniciado = true;

    const loginBtn = document.getElementById("login-btn");
    if (loginBtn) {
        loginBtn.disabled = true;
        loginBtn.textContent = "Entrando...";
    }

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

        <div class="guide-box">
            <div class="guide-box-header">
                <h2>📘 Como usar o bot</h2>
                <p>Guia rápido dos comandos mais importantes do painel.</p>
            </div>

            <div class="guide-grid">
                <section class="guide-section">
                    <h3>🎲 Comando de dados (roll)</h3>

                    <p class="guide-intro">
                        Rola dados avançados para RPG, testes e cálculos.
                        Aceita expressões como <code>1d20</code>, <code>2d6+3</code>, <code>4d10!</code>, <code>3#1d20+5</code> e combinações com operações matemáticas.
                    </p>

                    <h4>Símbolos do comando de dados</h4>

                    <div class="guide-symbol-list">
                        <div class="guide-symbol-item">
                            <div class="guide-symbol">d</div>
                            <div>
                                <strong>Define um dado.</strong>
                                <p>Ex: <code>1d20</code> = 1 dado de 20 lados; <code>2d6</code> = 2 dados de 6 lados.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">!</div>
                            <div>
                                <strong>Explosão de dado.</strong>
                                <p>Se o resultado atingir o limite, rola de novo e soma. Ex: <code>1d6!</code>.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">!N</div>
                            <div>
                                <strong>Explosão com limite personalizado.</strong>
                                <p>Ex: <code>1d10!8</code> explode em resultados 8, 9 ou 10.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">a</div>
                            <div>
                                <strong>Embaralha a ordem dos resultados.</strong>
                                <p>Em vez de ordenar do maior para o menor. Ex: <code>4d6a</code>.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">#</div>
                            <div>
                                <strong>Repete a mesma rolagem várias vezes.</strong>
                                <p>Ex: <code>3#1d20+5</code> faz 3 rolagens separadas de <code>1d20+5</code>.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">+ - * /</div>
                            <div>
                                <strong>Operações matemáticas básicas.</strong>
                                <p>Ex: <code>2d6+3</code>, <code>1d8*2</code>.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">()</div>
                            <div>
                                <strong>Agrupa partes da conta.</strong>
                                <p>Ex: <code>(2d6+4)*2</code>.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">^</div>
                            <div>
                                <strong>Potência.</strong>
                                <p>Ex: <code>2^3</code> = 8.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">z</div>
                            <div>
                                <strong>Raiz com índice.</strong>
                                <p>Ex: <code>9z2</code> = raiz quadrada de 9; <code>27z3</code> = raiz cúbica de 27.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">%</div>
                            <div>
                                <strong>Porcentagem de um valor.</strong>
                                <p>Ex: <code>50%200</code> = 100.</p>
                            </div>
                        </div>
                    </div>

                    <h4>Exemplos de uso</h4>
                    <div class="guide-example-list">
                        <span class="guide-example">1d20</span>
                        <span class="guide-example">2d6+3</span>
                        <span class="guide-example">4d10!</span>
                        <span class="guide-example">1d10!8</span>
                        <span class="guide-example">4d6a</span>
                        <span class="guide-example">3#1d20+5</span>
                        <span class="guide-example">(2d6+4)*2</span>
                    </div>
                </section>

                <section class="guide-section">
                    <h3>🧮 Comando de matemática (math)</h3>

                    <p class="guide-intro">
                        Resolve expressões matemáticas avançadas.
                        Aceita operações básicas, potência, raiz, porcentagem e parênteses. Não aceita dados.
                    </p>

                    <h4>Símbolos do comando de matemática</h4>

                    <div class="guide-symbol-list">
                        <div class="guide-symbol-item">
                            <div class="guide-symbol">+</div>
                            <div>
                                <strong>Soma.</strong>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">-</div>
                            <div>
                                <strong>Subtração.</strong>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">*</div>
                            <div>
                                <strong>Multiplicação.</strong>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">/</div>
                            <div>
                                <strong>Divisão.</strong>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">()</div>
                            <div>
                                <strong>Prioridade de cálculo.</strong>
                                <p>Ex: <code>(2+3)*4</code>.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">^</div>
                            <div>
                                <strong>Potência.</strong>
                                <p>Ex: <code>2^4</code> = 16.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">z</div>
                            <div>
                                <strong>Raiz com índice.</strong>
                                <p>Ex: <code>16z2</code> = 4; <code>27z3</code> = 3.</p>
                            </div>
                        </div>

                        <div class="guide-symbol-item">
                            <div class="guide-symbol">%</div>
                            <div>
                                <strong>Porcentagem de um valor.</strong>
                                <p>Ex: <code>25%200</code> = 50.</p>
                            </div>
                        </div>
                    </div>

                    <h4>Exemplos de uso</h4>
                    <div class="guide-example-list">
                        <span class="guide-example">2+2*5</span>
                        <span class="guide-example">(10+5)/3</span>
                        <span class="guide-example">2^3</span>
                        <span class="guide-example">16z2</span>
                        <span class="guide-example">25%200</span>
                    </div>
                </section>
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
    `;

    carregarListaTriggers();
}

function agruparTriggers(data) {
    const mapa = new Map();

    (data || []).forEach(item => {
        const nomeTrigger = String(item?.trigger ?? "").trim();
        if (!nomeTrigger) return;

        const responses = Array.isArray(item.responses) ? item.responses : [];

        if (!mapa.has(nomeTrigger)) {
            mapa.set(nomeTrigger, {
                trigger: nomeTrigger,
                responses: []
            });
        }

        mapa.get(nomeTrigger).responses.push(...responses);
    });

    return Array.from(mapa.values()).sort((a, b) =>
        a.trigger.localeCompare(b.trigger, "pt-BR", { sensitivity: "base" })
    );
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
        const triggersAgrupadas = agruparTriggers(data);

        container.innerHTML = "";

        if (!triggersAgrupadas.length) {
            container.innerHTML = `
                <div class="card">
                    <h3>Sem triggers</h3>
                    <p class="empty-text">Crie a primeira trigger para começar.</p>
                </div>
            `;
            return;
        }

        container.innerHTML = triggersAgrupadas.map(item => criarCardTrigger(item)).join("");
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
                type="button"
                class="trigger-delete-btn"
                title="Excluir trigger"
                onclick="event.stopPropagation(); abrirModalExcluirTrigger('${escaparJs(item.trigger)}')">
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
    `;

    try {
        const data = await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses`);
        const triggersAgrupadas = agruparTriggers(data);
        const item = triggersAgrupadas.find(x => String(x.trigger) === String(trigger));

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
                        type="button"
                        class="mini-delete-btn"
                        title="Excluir resposta"
                        onclick="event.stopPropagation(); abrirModalExcluirResponsePorBotao('${escaparJs(item.trigger)}', ${index}, this)">
                        🗑️
                    </button>
                </div>
            </div>
        `).join("")
        : `<p class="empty-text">Nenhuma response cadastrada para esta trigger.</p>`;

    return `
        <div class="card trigger-details-card">
            <button
                type="button"
                class="trigger-delete-btn"
                title="Excluir trigger"
                onclick="event.stopPropagation(); abrirModalExcluirTrigger('${escaparJs(item.trigger)}')">
                🗑️
            </button>

            <div class="trigger-header">
                <div class="trigger-header-main">
                    <h1 class="trigger-details-title">${escaparHtml(item.trigger)}</h1>
                    <p class="trigger-count">${totalResponses} response${totalResponses === 1 ? "" : "s"}</p>
                </div>
            </div>

            <div class="add-response-box details-add-response-box">
                <textarea
                    id="new-response-${escaparHtml(item.trigger)}"
                    class="edit-box new-response-textarea"
                    rows="3"
                    placeholder="Digite uma nova response para esta trigger"></textarea>

                <div class="add-response-actions">
                    <button
                        type="button"
                        class="secondary-btn"
                        onclick="adicionarResponse('${escaparJs(item.trigger)}')">
                        Adicionar response
                    </button>
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

async function adicionarResponse(trigger) {
    const textarea = document.getElementById(`new-response-${trigger}`);
    if (!textarea) return;

    const nova = textarea.value.trim();

    if (!nova) {
        alert("Digite uma response válida.");
        return;
    }

    try {
        await fetchJson(`${API_BASE}/api/guilds/${getServerId()}/responses/${encodeURIComponent(trigger)}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(nova)
        });

        textarea.value = "";
        await abrirDetalhesTrigger(trigger);
    } catch (err) {
        console.error("Erro ao adicionar response:", err);
        alert("Erro ao adicionar response.");
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

function abrirModalExcluirResponsePorBotao(trigger, index, button) {
    const container = button?.parentElement?.parentElement;
    const textarea = container?.querySelector(".response-edit-box");

    const texto = textarea ? textarea.value : "";

    abrirModalExcluirResponse(trigger, index, texto);
}

function abrirModalExcluirTrigger(trigger) {
    deleteData = {
        type: "trigger",
        trigger
    };

    const modal = document.getElementById("delete-modal");
    const title = document.getElementById("delete-modal-title");
    const message = document.getElementById("delete-modal-message");
    const preview = document.getElementById("delete-preview");

    if (!modal || !title || !message || !preview) {
        console.error("Modal de exclusão não encontrado no HTML.");
        return;
    }

    title.textContent = "Excluir trigger";
    message.textContent = "Tem certeza que deseja excluir esta trigger inteira?";
    preview.innerHTML = `<strong>Trigger:</strong><br>${escaparHtml(trigger)}`;

    modal.classList.remove("hidden");
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

    if (!modal || !title || !message || !preview) {
        console.error("Modal de exclusão não encontrado no HTML.");
        return;
    }

    title.textContent = "Excluir resposta";
    message.textContent = "Tem certeza que deseja excluir esta resposta?";
    preview.textContent = responseTexto ?? "";

    modal.classList.remove("hidden");
}

function fecharModalExcluir() {
    deleteData = null;

    const modal = document.getElementById("delete-modal");
    const preview = document.getElementById("delete-preview");

    if (preview) {
        preview.innerHTML = "";
    }

    modal?.classList.add("hidden");
}

async function confirmarExclusao() {
    if (!deleteData || !currentGuildId) {
        fecharModalExcluir();
        return;
    }

    try {
        if (deleteData.type === "trigger") {
            const data = await fetchJson(`${API_BASE}/api/guilds/${currentGuildId}/responses`);
            const repetidas = (data || []).filter(x => String(x.trigger) === String(deleteData.trigger));

            for (let i = 0; i < repetidas.length; i++) {
                await fetchJson(
                    `${API_BASE}/api/guilds/${currentGuildId}/responses/${encodeURIComponent(deleteData.trigger)}`,
                    { method: "DELETE" }
                );
            }

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

async function alterarStatusComando(commandName, enabled) {
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

function normalizarAliasesInput(valor) {
    return String(valor || "")
        .split(",")
        .map(x => x.trim())
        .filter(Boolean);
}

async function alterarAliasesComando(commandName) {
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

async function alterarCooldownComando(commandName) {
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

// =========================
// Moderação
// =========================

function carregarModeracao() {
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

function definirAbaModeracaoAtiva(tab) {
    document.getElementById("mod-tab-welcome")?.classList.remove("active");
    document.getElementById("mod-tab-roles")?.classList.remove("active");

    if (tab === "welcome") {
        document.getElementById("mod-tab-welcome")?.classList.add("active");
    }

    if (tab === "roles") {
        document.getElementById("mod-tab-roles")?.classList.add("active");
    }
}

async function abrirAbaModeracao(tab) {
    definirAbaModeracaoAtiva(tab);

    if (tab === "welcome") {
        await carregarModeracaoWelcomeLeave();
        return;
    }

    if (tab === "roles") {
        await carregarModeracaoRoles();
    }
}

async function carregarCanaisGuild() {
    return await fetchJson(`${API_BASE}/guilds/${getServerId()}/channels`);
}

async function carregarCargosGuild() {
    return await fetchJson(`${API_BASE}/guilds/${getServerId()}/roles`);
}

function criarOptionsSelect(items, selectedValue, placeholder) {
    const baseOption = `<option value="">${escaparHtml(placeholder)}</option>`;

    const options = (items || []).map(item => `
        <option value="${escaparHtml(item.id)}" ${String(item.id) === String(selectedValue ?? "") ? "selected" : ""}>
            ${escaparHtml(item.name)}
        </option>
    `).join("");

    return baseOption + options;
}

async function carregarModeracaoWelcomeLeave() {
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

async function salvarWelcomeLeave() {
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

async function carregarModeracaoRoles() {
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

function criarLinhaRegraCargo(reward = {}, index = 0, roles = []) {
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

async function adicionarRegraCargo() {
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

function removerRegraCargo(button) {
    const row = button.closest(".level-role-row");
    row?.remove();

    const list = document.getElementById("level-roles-list");
    if (list && !list.querySelector(".level-role-row")) {
        list.innerHTML = `<p class="empty-text">Nenhuma regra cadastrada.</p>`;
    }
}

async function salvarRoleRewards() {
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
    const btnModeracao = document.getElementById("btn-moderacao");

    if (loginBtn) {
        loginBtn.onclick = iniciarLoginDiscord;
    }

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

    if (btnModeracao) {
    btnModeracao.onclick = (e) => {
        e.preventDefault();
        carregarModeracao();
    };
}

    if (toggle && sidebar) {
        toggle.onclick = () => sidebar.classList.toggle("closed");
    }

    console.log("API_BASE atual:", API_BASE);

    mostrarTela("loading-screen");

    setTimeout(() => {
        const loadingScreen = document.getElementById("loading-screen");
        if (loadingScreen && !loadingScreen.classList.contains("hidden")) {
            console.warn("[AUTH] Loading demorou demais. Indo para auth-screen.");
            mostrarTela("auth-screen");
        }
    }, 9000);

    verificarSessao();

    setInterval(() => {
        if (!document.getElementById("app-screen")?.classList.contains("hidden")) {
            loadStats();
        }
    }, 5000);
});