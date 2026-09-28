// src/utils/errors.ts
import axios from "axios";

type ProblemBody = { detail?: unknown; title?: unknown; status?: unknown; error?: unknown };

export function getErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as ProblemBody | string | undefined;
    if (typeof data === "string") return data;

    if (data?.detail) return String(data.detail);
    if (data?.title && data?.status) return `${data.title} (${data.status})`;
    
    if (data?.error) return String(data.error);

    return err.message || "Request failed";
  }
  
  return (err as Error)?.message ?? "Unknown error";
}
