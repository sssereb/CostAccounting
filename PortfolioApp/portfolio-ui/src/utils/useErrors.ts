// src/utils/errors.ts
import axios from "axios";

export function getErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as any;
    
    if (data?.detail) return String(data.detail);
    if (data?.title && data?.status) return `${data.title} (${data.status})`;
    
    if (data?.error) return String(data.error);
    
    if (typeof data === "string") return data;
    
    return err.message || "Request failed";
  }
  
  return (err as Error)?.message ?? "Unknown error";
}
