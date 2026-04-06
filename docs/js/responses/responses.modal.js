import { API_BASE } from ".../core/config.js";
import { appState } from ".../core/state.js";
import { fetchJson } from ".../core/http.js";
import { escaparHtml } from ".../core/escape.js";
import { carregarServidor, abrirDetalhesTrigger } from "./responses.page.js";

export function abrirModalExcluirResponsePorBotao(trigger, index, button) {
    const container = button?.parentElement?.parentElement;
    const textarea = container?.querySelector(".response-edit-box");

    const texto = textarea ? textarea.value : "";

    abrirModalExcluirResponse(trigger, index, texto);
}

export function abrirModalExcluirTrigger(trigger) {
    appState.deleteData = {
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

export function abrirModalExcluirResponse(trigger, index, responseTexto) {
    appState.deleteData = {
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

export function fecharModalExcluir() {
    appState.deleteData = null;

    const modal = document.getElementById("delete-modal");
    const preview = document.getElementById("delete-preview");

    if (preview) {
        preview.innerHTML = "";
    }

    modal?.classList.add("hidden");
}

export async function confirmarExclusao() {
    if (!appState.deleteData || !appState.currentGuildId) {
        fecharModalExcluir();
        return;
    }

    try {
        if (appState.deleteData.type === "trigger") {
            const data = await fetchJson(`${API_BASE}/api/guilds/${appState.currentGuildId}/responses`);
            const repetidas = (data || []).filter(x => String(x.trigger) === String(appState.deleteData.trigger));

            for (let i = 0; i < repetidas.length; i++) {
                await fetchJson(
                    `${API_BASE}/api/guilds/${appState.currentGuildId}/responses/${encodeURIComponent(appState.deleteData.trigger)}`,
                    { method: "DELETE" }
                );
            }

            carregarServidor();
            return;
        }

        if (appState.deleteData.type === "response") {
            await fetchJson(
                `${API_BASE}/api/guilds/${appState.currentGuildId}/responses/${encodeURIComponent(appState.deleteData.trigger)}/${appState.deleteData.index}`,
                {
                    method: "DELETE"
                }
            );

            await abrirDetalhesTrigger(appState.deleteData.trigger);
            return;
        }
    } catch (err) {
        console.error("Erro ao excluir:", err);
        alert("Erro ao excluir.");
    } finally {
        fecharModalExcluir();
    }
}