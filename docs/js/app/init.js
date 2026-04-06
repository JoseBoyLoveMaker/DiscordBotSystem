import { API_BASE } from ".../core/config.js";
import { appState } from ".../core/state.js";
import { mostrarTela } from "../core/screens.js";
import { fetchJson } from ".../core/http.js";

import { preencherUsuario } from ".../user/user.ui.js";
import { carregarServidores } from "../guilds/guilds.ui.js";

import { carregarDashboard, loadStats, adicionarBotAoServidor } from "../dashboard/dashboard.page.js";

import {
    carregarServidor,
    abrirDetalhesTrigger,
    criarTrigger,
    adicionarResponse,
    editarResponse
} from "../responses/responses.page.js";

import {
    abrirModalExcluirResponsePorBotao,
    abrirModalExcluirTrigger,
    abrirModalExcluirResponse,
    fecharModalExcluir,
    confirmarExclusao
} from "../responses/responses.modal.js";

import {
    carregarComandos,
    alterarStatusComando,
    alterarAliasesComando,
    alterarCooldownComando
} from "../commands/commands.page.js";

import {
    carregarModeracao,
    abrirAbaModeracao,
    salvarWelcomeLeave,
    adicionarRegraCargo,
    removerRegraCargo,
    salvarRoleRewards
} from "../moderation/moderation.page.js";

async function verificarSessao() {
    try {
        console.log("[AUTH] Verificando sessão...");

        const user = await fetchJson(`${API_BASE}/auth/me`, { timeoutMs: 7000 });

        console.log("[AUTH] Sessão encontrada:", user);

        appState.currentUser = user;
        preencherUsuario(user);

        await carregarServidores();

        mostrarTela("app-screen");
        carregarDashboard();
    } catch (err) {
        console.error("[AUTH] Sessão não encontrada ou erro:", err);
        mostrarTela("auth-screen");
    }
}

function iniciarLoginDiscord() {
    if (appState.loginIniciado) return;

    appState.loginIniciado = true;

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

    appState.currentUser = null;
    appState.currentGuildId = null;
    mostrarTela("auth-screen");
}

function registrarGlobais() {
    window.carregarDashboard = carregarDashboard;
    window.adicionarBotAoServidor = adicionarBotAoServidor;

    window.carregarServidor = carregarServidor;
    window.abrirDetalhesTrigger = abrirDetalhesTrigger;
    window.criarTrigger = criarTrigger;
    window.adicionarResponse = adicionarResponse;
    window.editarResponse = editarResponse;

    window.abrirModalExcluirResponsePorBotao = abrirModalExcluirResponsePorBotao;
    window.abrirModalExcluirTrigger = abrirModalExcluirTrigger;
    window.abrirModalExcluirResponse = abrirModalExcluirResponse;
    window.fecharModalExcluir = fecharModalExcluir;
    window.confirmarExclusao = confirmarExclusao;

    window.carregarComandos = carregarComandos;
    window.alterarStatusComando = alterarStatusComando;
    window.alterarAliasesComando = alterarAliasesComando;
    window.alterarCooldownComando = alterarCooldownComando;

    window.carregarModeracao = carregarModeracao;
    window.abrirAbaModeracao = abrirAbaModeracao;
    window.salvarWelcomeLeave = salvarWelcomeLeave;
    window.adicionarRegraCargo = adicionarRegraCargo;
    window.removerRegraCargo = removerRegraCargo;
    window.salvarRoleRewards = salvarRoleRewards;
}

document.addEventListener("DOMContentLoaded", () => {
    registrarGlobais();

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

// =========================
// GLOBAL (onclick HTML)
// =========================

window.criarTrigger = criarTrigger;
window.abrirDetalhesTrigger = abrirDetalhesTrigger;
window.adicionarResponse = adicionarResponse;
window.editarResponse = editarResponse;

window.abrirModalExcluirResponsePorBotao = abrirModalExcluirResponsePorBotao;
window.abrirModalExcluirTrigger = abrirModalExcluirTrigger;
window.abrirModalExcluirResponse = abrirModalExcluirResponse;
window.fecharModalExcluir = fecharModalExcluir;
window.confirmarExclusao = confirmarExclusao;

window.carregarServidor = carregarServidor;

// comandos
window.alterarStatusComando = alterarStatusComando;
window.alterarAliasesComando = alterarAliasesComando;
window.alterarCooldownComando = alterarCooldownComando;

// moderação
window.abrirAbaModeracao = abrirAbaModeracao;
window.salvarWelcomeLeave = salvarWelcomeLeave;
window.adicionarRegraCargo = adicionarRegraCargo;
window.removerRegraCargo = removerRegraCargo;
window.salvarRoleRewards = salvarRoleRewards;