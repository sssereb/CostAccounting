// src/hooks/useFeeRules.ts
//
// React-Query хуки для новых конечных точек:
//
//   GET    /fees/get       – получить список правил
//   PUT    /fees/put       – заменить список
//   DELETE /fees/delete    – очистить все правила
//
// Использует общий axios-инстанс `http` из src/lib/http.ts.
//
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { http } from "../lib/http";

/* ---------- типы DTO ---------- */
export type FeeType = "FixedPerTrade" | "FixedPerShare" | "PercentOfValue";
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

/* ---------- PUT  /fees/put ---------- */
export function useSaveFeeRules() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (payload: FeeRuleDto[]) =>
      http.put("/fees/put", payload),
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
