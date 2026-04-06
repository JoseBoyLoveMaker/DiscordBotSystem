export function preencherUsuario(user) {
    const nameEl = document.getElementById("user-name");
    const avatarEl = document.getElementById("user-avatar");

    if (nameEl) {
        nameEl.textContent = user.globalName || user.username || "Usuário";
    }

    if (avatarEl && user.avatarUrl) {
        avatarEl.src = user.avatarUrl;
    }
}