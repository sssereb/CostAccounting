// portfolio-ui/src/lib/http.ts
import axios from "axios";

export const http = axios.create({
  baseURL: "/api",                      // proxy prefix, see vite.config.ts
  headers: { "Content-Type": "application/json" }
});
