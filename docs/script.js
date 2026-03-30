const SERVER_ID = "562767536899358732";
const API_BASE = "https://localhost:7229";

let isBold = false;
let isItalic = false;
let deleteData = null;

// =========================
// FUNÇÕES DE NAVEGAÇÃO
// =========================

function carregarComandos() {
    const content = document.getElementById("main-content");

    content.innerHTML = `
        <div class="header-trigger">
            <div>
                <h1>Comandos</h1>
                <p>Gerencie comandos, aliases e atalhos.</p>
            </div>
        </div>

        <div id="commands-list" class="dashboard">
            <p>Carregando comandos...</p>
        </div>
    `;

    carregarListaComandos();
}

function carregarDashboard() {
    const content = document.getElementById("main-content");

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

function carregarServidor() {
    const content = document.getElementById("main-content");

    content.innerHTML = `
        <h1>Servidor</h1>

        <div class="dashboard">
            <div class="card">
                <h3>👥 Membros</h3>
                <p id="members">Carregando...</p>
            </div>

            <div class="card">
                <h3>💬 Respostas automáticas</h3>
                <button onclick="abrirRespostas()">Gerenciar</button>
            </div>

            <div class="card">
                <h3>⚙️ Configurações</h3>
                <button>Editar</button>
            </div>
        </div>
    `;
}

// =========================
// TRIGGERS / RESPOSTAS
// =========================

function abrirRespostas() {
    const content = document.getElementById("main-content");

    content.innerHTML = `
        <div class="header-trigger">
            <h1>Respostas Automáticas</h1>
            <button onclick="criarTrigger()" class="btn-add">➕ Nova Trigger</button>
        </div>

        <div id="triggers-container" class="dashboard">
            <p>Carregando triggers...</p>
        </div>
    `;

    carregarTriggers();
}

async function carregarTriggers() {
    const container = document.getElementById("triggers-container");

    try {
        const res = await fetch(`${API_BASE}/api/responses`);

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        const data = await res.json();
        container.innerHTML = "";

        data.forEach(trigger => {
            container.innerHTML += `
                <div class="card" onclick="abrirTrigger(\`${trigger.trigger}\`)">
                    <h3>⚡ ${trigger.trigger}</h3>
                    <p>${trigger.responses?.length || 0} respostas</p>
                </div>
            `;
        });
    } catch (err) {
        console.error("Erro ao carregar triggers:", err);
        container.innerHTML = `<p style="color:red;">Erro ao carregar triggers.</p>`;
    }
}

async function abrirTrigger(triggerName) {
    const content = document.getElementById("main-content");

    try {
        const res = await fetch(`${API_BASE}/api/responses/${encodeURIComponent(triggerName)}`);

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        const data = await res.json();

        content.innerHTML = `
            <div class="header-trigger">
                <div>
                    <h1>Trigger: ${triggerName}</h1>
                    <button onclick="abrirRespostas()">⬅ Voltar</button>
                </div>

                <div class="format-buttons">
                    <button onclick="toggleBold('${triggerName}')" class="btn-format">B</button>
                    <button onclick="toggleItalic('${triggerName}')" class="btn-format">I</button>
                    <button onclick="adicionarResposta(\`${triggerName}\`)" class="btn-add">
                        ➕ Nova resposta
                    </button>
                </div>
            </div>

            <div id="responses-list" class="dashboard"></div>

            <button onclick="adicionarResposta(\`${triggerName}\`)" class="btn-add">
                ➕ Nova resposta
            </button>
        `;

        const list = document.getElementById("responses-list");

        data.responses.forEach((resp, index) => {
            list.innerHTML += `
                <div class="card">
                    <textarea
                        class="edit-box"
                        oninput="autoResize(this)"
                        onblur="salvarEdicao('${triggerName}', ${index}, this.value)"
                    >${resp.replace(/</g, "&lt;")}</textarea>

                    <button class="delete-btn" onclick="confirmarDelete('${triggerName}', ${index})">🗑️</button>
                </div>
            `;
        });

        setTimeout(() => {
            document.querySelectorAll(".edit-box").forEach(autoResize);
        }, 0);

        updateButtons();
    } catch (err) {
        console.error("Erro ao abrir trigger:", err);
        content.innerHTML = `<p style="color:red;">Erro ao carregar a trigger.</p>`;
    }
}

async function criarTrigger() {
    const nome = prompt("Nome da nova trigger:");
    if (!nome) return;

    try {
        const res = await fetch(`${API_BASE}/api/responses`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(nome)
        });

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        carregarTriggers();
    } catch (err) {
        console.error("Erro ao criar trigger:", err);
        alert("Não foi possível criar a trigger.");
    }
}

async function deletarResposta(trigger, index) {
    try {
        const res = await fetch(`${API_BASE}/api/responses/${encodeURIComponent(trigger)}/${index}`, {
            method: "DELETE"
        });

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        abrirTrigger(trigger);
    } catch (err) {
        console.error("Erro ao deletar resposta:", err);
        alert("Não foi possível deletar a resposta.");
    }
}

async function editarResposta(trigger, index) {
    const nova = prompt("Editar resposta:");
    if (!nova) return;

    try {
        const res = await fetch(`${API_BASE}/api/responses/${encodeURIComponent(trigger)}/${index}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(nova)
        });

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        abrirTrigger(trigger);
    } catch (err) {
        console.error("Erro ao editar resposta:", err);
        alert("Não foi possível editar a resposta.");
    }
}

async function salvarEdicao(trigger, index, nova) {
    try {
        const res = await fetch(`${API_BASE}/api/responses/${encodeURIComponent(trigger)}/${index}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(nova)
        });

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }
    } catch (err) {
        console.error("Erro ao salvar edição:", err);
    }
}

async function adicionarResposta(trigger) {
    const list = document.getElementById("responses-list");
    const card = document.createElement("div");
    card.className = "card";

    card.innerHTML = `
        <textarea
            class="edit-box"
            placeholder="Digite a nova resposta..."
        ></textarea>
    `;

    list.prepend(card);

    const textarea = card.querySelector("textarea");
    textarea.focus();
    autoResize(textarea);

    textarea.addEventListener("input", () => autoResize(textarea));

    textarea.addEventListener("blur", async () => {
        const value = textarea.value.trim();

        if (!value) {
            card.remove();
            return;
        }

        try {
            const res = await fetch(`${API_BASE}/api/responses/${encodeURIComponent(trigger)}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(value)
            });

            if (!res.ok) {
                throw new Error(`Erro HTTP: ${res.status}`);
            }

            abrirTrigger(trigger);
        } catch (err) {
            console.error("Erro ao adicionar resposta:", err);
            alert("Não foi possível adicionar a resposta.");
        }
    });
}

// =========================
// MODAL
// =========================

function confirmarDelete(trigger, index) {
    const textareas = document.querySelectorAll(".edit-box");
    const texto = textareas[index]?.value || "(vazio)";

    deleteData = { trigger, index };

    const preview = document.getElementById("delete-preview");
    preview.innerText = texto;

    const modal = document.getElementById("confirm-modal");
    modal.classList.remove("hidden");
}

function fecharModal() {
    document.getElementById("confirm-modal").classList.add("hidden");
    deleteData = null;
}

// =========================
// FORMATAÇÃO
// =========================

function autoResize(textarea) {
    textarea.style.height = "0px";
    textarea.style.height = textarea.scrollHeight + "px";
}

function toggleBold(trigger) {
    isBold = !isBold;
    atualizarTodasRespostas(trigger);
    updateButtons();
}

function toggleItalic(trigger) {
    isItalic = !isItalic;
    atualizarTodasRespostas(trigger);
    updateButtons();
}

function limparFormatacao(texto) {
    return texto
        .replace(/^\*{1,3}/, "")
        .replace(/\*{1,3}$/, "");
}

function aplicarFormatacao(texto) {
    const limpo = limparFormatacao(texto);

    if (isBold && isItalic) return `***${limpo}***`;
    if (isBold) return `**${limpo}**`;
    if (isItalic) return `*${limpo}*`;

    return limpo;
}

async function atualizarTodasRespostas(trigger) {
    const textareas = document.querySelectorAll(".edit-box");

    for (let i = 0; i < textareas.length; i++) {
        const texto = textareas[i].value;
        const formatado = aplicarFormatacao(texto);

        textareas[i].value = formatado;
        autoResize(textareas[i]);

        try {
            const res = await fetch(`${API_BASE}/api/responses/${encodeURIComponent(trigger)}/${i}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(formatado)
            });

            if (!res.ok) {
                throw new Error(`Erro HTTP: ${res.status}`);
            }
        } catch (err) {
            console.error(`Erro ao atualizar resposta ${i}:`, err);
        }
    }
}

function updateButtons() {
    const buttons = document.querySelectorAll(".btn-format");
    if (buttons.length < 2) return;

    buttons[0].classList.toggle("active", isBold);
    buttons[1].classList.toggle("active", isItalic);
}

// =========================
// DASHBOARD
// =========================

async function loadStats() {
    const usersEl = document.getElementById("users");
    const xpEl = document.getElementById("xp");
    const commandsEl = document.getElementById("commands");
    const statusEl = document.getElementById("status");

    if (!usersEl && !xpEl && !commandsEl && !statusEl) return;

    try {
        const res = await fetch(`${API_BASE}/api/bot/stats`);

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        const data = await res.json();

        if (usersEl) usersEl.innerText = data.users ?? 0;
        if (xpEl) xpEl.innerText = data.totalXp ?? 0;
        if (commandsEl) commandsEl.innerText = data.commands ?? 0;

        if (statusEl) {
            if (data.status === "Online") {
                statusEl.innerText = "🟢 Online";
                statusEl.style.color = "lime";
            } else {
                statusEl.innerText = "🔴 Offline";
                statusEl.style.color = "red";
            }
        }
    } catch (err) {
        console.error("Erro ao carregar stats:", err);

        if (statusEl) {
            statusEl.innerText = "Erro ao conectar API";
            statusEl.style.color = "red";
        }
    }
}
// =========================
// COMANDOS
// =========================

async function carregarListaComandos() {
    const container = document.getElementById("commands-list");

    try {
        const res = await fetch(`${API_BASE}/api/commands/${SERVER_ID}`);

        if (!res.ok) {
            throw new Error(`Erro HTTP: ${res.status}`);
        }

        const data = await res.json();
        container.innerHTML = "";

        if (!data || data.length === 0) {
            container.innerHTML = `<p>Nenhum comando encontrado para este servidor.</p>`;
            return;
        }

        data.forEach(cmd => {
            container.innerHTML += `
                <div class="card">
                    <h3>⚡ ${cmd.commandName}</h3>
                    <p style="font-size:14px; font-weight:normal;">${cmd.description ?? ""}</p>

                    <label style="margin-top:10px;">
                        <input type="checkbox"
                               ${cmd.enabled ? "checked" : ""}
                               onchange="alterarStatusComando('${cmd.commandName}', this.checked)">
                        Ativado
                    </label>

                    <div style="margin-top:10px;">
                        <strong>Aliases:</strong>
                        <textarea class="edit-box"
                            onblur="salvarAliases('${cmd.commandName}', this.value)">${(cmd.aliases || []).join(", ")}</textarea>
                    </div>

                    <div style="margin-top:10px;">
                        <strong>Cooldown:</strong>
                        <input type="number"
                               value="${cmd.cooldownSeconds ?? 0}"
                               min="0"
                               onblur="salvarCooldown('${cmd.commandName}', this.value)">
                    </div>
                </div>
            `;
        });

        setTimeout(() => {
            document.querySelectorAll(".edit-box").forEach(autoResize);
        }, 0);
    } catch (err) {
        console.error("Erro ao carregar comandos:", err);
        container.innerHTML = `<p style="color:red;">Erro ao carregar comandos.</p>`;
    }
}

async function alterarStatusComando(commandName, enabled) {
    await fetch(`${API_BASE}/api/commands/${SERVER_ID}/${encodeURIComponent(commandName)}/enabled`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(enabled)
    });
}

async function salvarAliases(commandName, valor) {
    const aliases = valor
        .split(",")
        .map(x => x.trim())
        .filter(x => x.length > 0);

    await fetch(`${API_BASE}/api/commands/${SERVER_ID}/${encodeURIComponent(commandName)}/aliases`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(aliases)
    });
}

async function salvarCooldown(commandName, valor) {
    const cooldown = parseInt(valor) || 0;

    await fetch(`${API_BASE}/api/commands/${SERVER_ID}/${encodeURIComponent(commandName)}/cooldown`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(cooldown)
    });
}

// =========================
// MENU
// =========================

function fecharMenu() {
    const sidebar = document.querySelector(".sidebar");
    if (sidebar) {
        sidebar.classList.remove("closed");
    }
}

// =========================
// INICIALIZAÇÃO
// =========================

document.addEventListener("DOMContentLoaded", () => {
    const sidebar = document.querySelector(".sidebar");
    const toggle = document.getElementById("menu-toggle");
    const btnDashboard = document.getElementById("btn-dashboard");
    const btnServidor = document.getElementById("btn-servidor");
    const btnComandos = document.getElementById("btn-comandos");
    const confirmDeleteBtn = document.getElementById("confirm-delete");

    if (btnDashboard) {
        btnDashboard.addEventListener("click", (e) => {
            e.preventDefault();
            carregarDashboard();
        });
    }

    if (btnServidor) {
        btnServidor.addEventListener("click", (e) => {
            e.preventDefault();
            carregarServidor();
        });
    }

    if (btnComandos) {
        btnComandos.addEventListener("click", (e) => {
            e.preventDefault();
            carregarComandos();
        });
    }

    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener("click", async () => {
            if (!deleteData) return;

            await deletarResposta(deleteData.trigger, deleteData.index);
            fecharModal();
        });
    }

    if (toggle && sidebar) {
        toggle.addEventListener("click", () => {
            sidebar.classList.toggle("closed");
        });
    }

    loadStats();
    setInterval(loadStats, 5000);
});