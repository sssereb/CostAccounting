// portfolio-ui/src/lib/http.ts
import axios from "axios";

export const http = axios.create({
  baseURL: "/api",                      // ← прокси-префикс, см. vite.config.ts
  headers: { "Content-Type": "application/json" }
});
