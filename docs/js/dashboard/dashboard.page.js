    import { API_BASE, BOT_CLIENT_ID, BOT_PERMISSIONS } from ".../core/config.js";
import { appState } from ".../core/state.js";
import { fetchJson } from ".../core/http.js";

export async function loadStats() {
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

export function carregarDashboard() {
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

export function adicionarBotAoServidor() {
    if (!appState.currentGuildId) {
        alert("Selecione um servidor primeiro.");
        return;
    }

    const inviteUrl =
        `https://discord.com/oauth2/authorize` +
        `?client_id=${BOT_CLIENT_ID}` +
        `&permissions=${BOT_PERMISSIONS}` +
        `&scope=bot%20applications.commands` +
        `&guild_id=${appState.currentGuildId}` +
        `&disable_guild_select=true`;

    window.open(inviteUrl, "_blank");
}