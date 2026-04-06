import { API_BASE } from ".../core/config.js";
import { fetchJson } from ".../core/http.js";
import { escaparHtml, escaparJs } from ".../core/escape.js";
import { getServerId } from "../guilds/guilds.ui.js";
import { agruparTriggers } from "./responses.helpers.js";

export function carregarServidor() {
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

export async function carregarListaTriggers() {
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

export function criarCardTrigger(item) {
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

export async function abrirDetalhesTrigger(trigger) {
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

export function criarDetalhesTrigger(item) {
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

export function configurarAutoSaveResponses(trigger, responses) {
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

export async function criarTrigger() {
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

export async function adicionarResponse(trigger) {
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

export async function editarResponse(trigger, index) {
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