export function mostrarTela(id) {
    document.getElementById("loading-screen")?.classList.add("hidden");
    document.getElementById("auth-screen")?.classList.add("hidden");
    document.getElementById("app-screen")?.classList.add("hidden");

    document.getElementById(id)?.classList.remove("hidden");
}