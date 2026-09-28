import { http } from "../lib/http";
import { useDebounce } from "../utils/useDebounce";
import type { AxiosError } from "axios";
// src/hooks/useTrades.ts
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";




/* ---------- DTO types ---------- */

export type BuyRequestDto = {
  assetId?: string;
  ticker:   string;
  qty:      number;
  price:    number;
  date:     string;
};
export type SellRequestDto = {
  assetId?: string;
  qty:     number;
  price:   number;
  method:  "FIFO" | "LIFO" | "Average";
  date:    string;
};

/* ---------- hook: resolve assetId by ticker ---------- */
export function useAssetId(ticker: string) {
  const debounced = useDebounce(ticker.trim().toUpperCase(), 400);

  return useQuery({
    queryKey: ["assetId", debounced],
    queryFn: () =>
      http.get<string>(`/assets/id/${debounced}`).then(r => r.data),
    enabled: !!debounced,
    retry: (failureCount, error) => {
      // retry at most once, and only on 5xx
      if (failureCount >= 1) return false;
      const status = (error as AxiosError).response?.status ?? 0;
      return status >= 500;
    }
  });
}


/* ---------- hook: BUY ---------- */
export function useBuy() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (p: BuyRequestDto) => http.post("/trades/buy", p),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["lots"] });     // refresh the lots grid
      qc.invalidateQueries({ queryKey: ["assets"] });   // and the assets list
    }
  });
}


/* ---------- hook: SELL ---------- */
export function useSell() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (p: SellRequestDto) => http.post("/trades/sell", p),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["lots"] });
      qc.invalidateQueries({ queryKey: ["assets"] });
    }
  });
}

export function useAssets() { 
  return useQuery({ queryKey: ["assets"], queryFn: () => http.get<Asset[]>("/assets").then(r => r.data) }); 
} 

export type Asset = { ticker: string, qtyRemaining: number, lastPrice: number };

export type Lot = { ticker:string; purchaseDate:string; qtyRemain:number; unitCost:number }; 

export function useLots() 
{ 
  return useQuery({ queryKey: ["lots"], queryFn: () => http.get<Lot[]>("/lots").then(r => r.data) }); 
}

export type Trade = { id:string; ticker:string; date:string; quantity:number; price:number; profitGross?:number; profitNet?:number };

export function useTrades() 
{ 
  return useQuery({ queryKey: ["trades"], queryFn: () => http.get<Trade[]>("/trades/all").then(r => r.data) }); 
}
