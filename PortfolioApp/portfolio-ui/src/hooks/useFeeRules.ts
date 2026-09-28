// src/hooks/useFeeRules.ts
// React Query hooks for the fee rules endpoints.
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { http } from "../lib/http";

/* ---------- DTO types ---------- */
export type FeeType = "FixedPerTrade" | "FixedPerShare" | "Percent";
export type FeeDirection = "Buy" | "Sell" | "Both";

export interface FeeRuleDto {
  type: FeeType;
  amount: number;
  direction: FeeDirection;
}

/* ---------- GET  /fees/get ---------- */
export function useFeeRules() {
  return useQuery({
    queryKey: ["fees"],
    queryFn : () =>
      http.get<FeeRuleDto[]>("/fees/get").then(r => r.data)
  });
}

/* ---------- POST /fees/save ---------- */
export function useSaveFeeRules() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (payload: FeeRuleDto[]) =>
      http.post("/fees/save", payload),
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: ["fees"] })
  });
}

/* ---------- DELETE  /fees/delete ---------- */
export function useResetFeeRules() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: () => http.delete("/fees/delete"),
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: ["fees"] })
  });
}
