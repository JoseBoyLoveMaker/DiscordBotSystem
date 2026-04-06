export function agruparTriggers(data) {
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