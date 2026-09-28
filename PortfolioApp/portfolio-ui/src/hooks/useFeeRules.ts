import { useQuery } from "@tanstack/react-query";
import { api, unwrap, type Schemas } from "../api/client";

export type FeeRuleDto = Schemas["FeeRuleDto"];

export function useFeeRules() {
  return useQuery({
    queryKey: ["fees"],
    queryFn: () => unwrap(api.GET("/api/fees/get")),
  });
}
