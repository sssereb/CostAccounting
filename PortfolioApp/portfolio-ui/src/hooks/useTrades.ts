import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api, ApiError, unwrap, type Schemas } from "../api/client";

export type Asset = Schemas["AssetDto"];
export type Lot = Schemas["LotDto"];
export type Trade = Schemas["TradeDto"];
export type BuyRequestDto = Schemas["BuyRequestDto"];
export type SellRequestDto = Schemas["SellRequestDto"];
export type CostBasisMethod = Schemas["CostBasisMethod"];

/** Looks up the asset id for an already debounced, upper-cased ticker. */
export function useAssetId(ticker: string) {
  return useQuery({
    queryKey: ["assetId", ticker],
    queryFn: () => unwrap(api.GET("/api/assets/id/{ticker}", { params: { path: { ticker } } })),
    enabled: !!ticker,
    // retry at most once, and only on 5xx
    retry: (failureCount, error) => failureCount < 1 && error instanceof ApiError && error.status >= 500,
  });
}

/* ---------- hook: BUY ---------- */
export function useBuy() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (body: BuyRequestDto) => unwrap(api.POST("/api/trades/buy", { body })),
    onSuccess: () => invalidatePortfolio(qc),
  });
}

/* ---------- hook: SELL ---------- */
export function useSell() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (body: SellRequestDto) => unwrap(api.POST("/api/trades/sell", { body })),
    onSuccess: () => invalidatePortfolio(qc),
  });
}

function invalidatePortfolio(qc: ReturnType<typeof useQueryClient>) {
  qc.invalidateQueries({ queryKey: ["lots"] });
  qc.invalidateQueries({ queryKey: ["assets"] });
  qc.invalidateQueries({ queryKey: ["trades"] });
}

export function useAssets() {
  return useQuery({ queryKey: ["assets"], queryFn: () => unwrap(api.GET("/api/assets")) });
}

export function useLots() {
  return useQuery({ queryKey: ["lots"], queryFn: () => unwrap(api.GET("/api/lots")) });
}

export function useTrades() {
  return useQuery({ queryKey: ["trades"], queryFn: () => unwrap(api.GET("/api/trades/all")) });
}
