import { http } from "../lib/http";
import { useDebounce } from "./useDebounce";   // относительный путь из той же папки
// src/hooks/useTrades.ts
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";




/* ---------- типы DTO ---------- */

export type BuyRequestDto = {
  assetId?: string;
  ticker:   string;
  qty:      number;
  price:    number;
  date:     string;
};
export type SellRequestDto = {
  assetId: string;
  qty:     number;
  price:   number;
  method:  "FIFO" | "LIFO" | "AverageCost";
  date:    string;
};

/* ---------- хук: получить assetId по тикеру ---------- */
export function useAssetId(ticker: string) {
  const debounced = useDebounce(ticker.trim().toUpperCase(), 400);

  return useQuery({
    queryKey: ["assetId", debounced],
    queryFn: () =>
      http.get<string>(`/assets/id/${debounced}`).then(r => r.data),
    enabled: !!debounced,
    retry: (failureCount, error) => {
      // повторим максимум 1 раз И только если это 5xx
      if (failureCount >= 1) return false;
      const status = (error as any).response?.status;
      return status >= 500;          // true → попробовать ещё раз
    }
  });
}


/* ---------- хук: BUY ---------- */
export function useBuy() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (p: BuyRequestDto) => http.post("/trades/buy", p),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["lots"] });     // ⚡ обновить грид
      qc.invalidateQueries({ queryKey: ["assets"] });   // заодно список активов
    }
  });
}


/* ---------- хук: SELL ---------- */
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

export type Asset = { id: string; ticker: string };

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
