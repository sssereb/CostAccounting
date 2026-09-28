// Refreshes openapi.json from a running backend: dotnet run --project PortfolioApp/PortfolioApp.Web
import { writeFile } from "node:fs/promises";

const url = process.env.OPENAPI_URL ?? "http://localhost:5255/swagger/v1/swagger.json";
const res = await fetch(url);
if (!res.ok) throw new Error(`GET ${url} failed with ${res.status}`);

await writeFile(new URL("../openapi.json", import.meta.url), JSON.stringify(await res.json(), null, 2) + "\n");
console.log(`openapi.json updated from ${url}`);
