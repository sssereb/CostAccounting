import createClient from "openapi-fetch";
import type { components, paths } from "./schema";

export type Schemas = components["schemas"];

export const api = createClient<paths>({ baseUrl: "/api" });

export class ApiError extends Error {
  readonly status: number;
  readonly body: unknown;

  constructor(status: number, body: unknown) {
    super(describeBody(body) ?? `Request failed (${status})`);
    this.name = "ApiError";
    this.status = status;
    this.body = body;
  }
}

type ProblemBody = { detail?: unknown; title?: unknown; status?: unknown; error?: unknown };

function describeBody(body: unknown): string | undefined {
  if (typeof body === "string") return body || undefined;
  if (!body || typeof body !== "object") return undefined;

  const p = body as ProblemBody;
  if (p.detail) return String(p.detail);
  if (p.title && p.status) return `${p.title} (${p.status})`;
  if (p.error) return String(p.error);
  return undefined;
}

/** Resolves to the response data, or throws ApiError so React Query treats non-2xx responses as failures. */
export async function unwrap<T>(request: Promise<{ data?: T; error?: unknown; response: Response }>): Promise<T> {
  const { data, error, response } = await request;
  if (!response.ok) throw new ApiError(response.status, error);
  return data as T;
}
