export const LOCAL_API_BASE = "https://localhost:7229";
export const PROD_API_BASE = "https://discordbotsystem-production.up.railway.app";

export const API_BASE =
    window.location.hostname === "localhost" ||
    window.location.hostname === "127.0.0.1"
        ? LOCAL_API_BASE
        : PROD_API_BASE;

export const BOT_CLIENT_ID = "1479279369477423135";
export const BOT_PERMISSIONS = "8";