export async function fetchJson(url, options = {}) {
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