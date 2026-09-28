// React Query hooks for the fee rules endpoints.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api, unwrap, type Schemas } from "../api/client";

export type FeeRuleDto = Schemas["FeeRuleDto"];
export type FeeType = Schemas["FeeType"];
export type FeeDirection = Schemas["FeeDirection"];

export function useFeeRules() {
  return useQuery({
    queryKey: ["fees"],
    queryFn: () => unwrap(api.GET("/api/fees/get")),
  });
}

export function useSaveFeeRules() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (body: FeeRuleDto[]) => unwrap(api.POST("/api/fees/save", { body })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["fees"] }),
  });
}

export function useResetFeeRules() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: () => unwrap(api.DELETE("/api/fees/delete")),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["fees"] }),
  });
}
