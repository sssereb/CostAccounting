import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api, ApiError, unwrap, type Schemas } from "../api/client";
import { useDebounce } from "../utils/useDebounce";

export type Asset = Schemas["AssetDto"];
export type Lot = Schemas["LotDto"];
export type Trade = Schemas["TradeDto"];
export type BuyRequestDto = Schemas["BuyRequestDto"];
export type SellRequestDto = Schemas["SellRequestDto"];
export type CostBasisMethod = Schemas["CostBasisMethod"];

/* ---------- hook: resolve assetId by ticker ---------- */
export function useAssetId(ticker: string) {
  const debounced = useDebounce(ticker.trim().toUpperCase(), 400);

  return useQuery({
    queryKey: ["assetId", debounced],
    queryFn: () => unwrap(api.GET("/assets/id/{ticker}", { params: { path: { ticker: debounced } } })),
    enabled: !!debounced,
    // retry at most once, and only on 5xx
    retry: (failureCount, error) => failureCount < 1 && error instanceof ApiError && error.status >= 500,
  });
}

/* ---------- hook: BUY ---------- */
export function useBuy() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (body: BuyRequestDto) => unwrap(api.POST("/trades/buy", { body })),
    onSuccess: () => invalidatePortfolio(qc),
  });
}

/* ---------- hook: SELL ---------- */
export function useSell() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (body: SellRequestDto) => unwrap(api.POST("/trades/sell", { body })),
    onSuccess: () => invalidatePortfolio(qc),
  });
}

function invalidatePortfolio(qc: ReturnType<typeof useQueryClient>) {
  qc.invalidateQueries({ queryKey: ["lots"] });
  qc.invalidateQueries({ queryKey: ["assets"] });
  qc.invalidateQueries({ queryKey: ["trades"] });
}

export function useAssets() {
  return useQuery({ queryKey: ["assets"], queryFn: () => unwrap(api.GET("/assets")) });
}

export function useLots() {
  return useQuery({ queryKey: ["lots"], queryFn: () => unwrap(api.GET("/lots")) });
}

export function useTrades() {
  return useQuery({ queryKey: ["trades"], queryFn: () => unwrap(api.GET("/trades/all")) });
}
