// src/components/BuyForm.tsx
import { useState } from "react";
import {
  Stack,
  TextField,
  Button,
  Autocomplete,
  Box,
  Alert,
} from "@mui/material";
import {
  useBuy,
  useAssetId,
  useAssets,
  type Asset,
} from "../hooks/useTrades";          // подправь путь если у тебя иначе
import type { BuyRequestDto } from "../hooks/useTrades"; // подправь путь если нужно
import { useDebounce } from "../utils/useDebounce"; // подправь путь если нужно

// Можно вынести в src/utils/date.ts
const todayIsoDate = () => new Date().toISOString().slice(0, 10); // "YYYY-MM-DD"
const isoFromDateInput = (yyyyMmDd: string) => `${yyyyMmDd}T00:00:00`;

export default function BuyForm() {
  const [ticker, setTicker] = useState("");
  const [qty, setQty] = useState<number>(0);
  const [price, setPrice] = useState<number>(0);
  const [date, setDate] = useState<string>(todayIsoDate());

  // все активы для выпадающего списка
  const assetsQ = useAssets();
  const assets = (assetsQ.data ?? []) as Asset[];

  // по введённому тикеру ищем assetId (если уже существует)
  const debouncedTicker = useDebounce(ticker.trim().toUpperCase(), 300);
  const assetIdQ = useAssetId(debouncedTicker);
  const buy = useBuy();

  const canBuy =
    debouncedTicker.length > 0 && qty > 0 && price > 0 && !buy.isPending;

  const handleBuy = () => {
    if (!canBuy) return;

    const base = { ticker, qty, price, date: isoFromDateInput(date) };

    // если нашли существующий assetId — шлём его; иначе — шлём тикер для создания
    const dto: BuyRequestDto = assetIdQ.data
      ? { ...base, assetId: assetIdQ.data }
      : { ...base, ticker: debouncedTicker };

    buy.mutate(dto);
  };

  // текущее выбранное значение для Autocomplete (объект)
  const selectedAsset =
    assets.find((a) => a.ticker.toUpperCase() === ticker.toUpperCase()) ?? null;

  const willCreate =
    !!ticker && !assetIdQ.isLoading && (assetIdQ.data ?? "") === "";

  return (
    <Stack direction="row" spacing={2} alignItems="center" useFlexGap flexWrap="wrap">
      {/* Ticker как выпадающий список + свободный ввод */}
      <Autocomplete<Asset, false, false, true>
        freeSolo
        options={assets}
        value={selectedAsset}
        loading={assetsQ.isLoading}
        getOptionLabel={(o) => (typeof o === "string" ? o : o.ticker)}
        isOptionEqualToValue={(opt, val) => opt.ticker === val.ticker}
        onChange={(_, val) => setTicker((val as Asset | null)?.ticker?.toUpperCase() ?? "")}
        inputValue={ticker}
        onInputChange={(_, val) => setTicker((val ?? "").toUpperCase())}
        renderOption={(props, opt) => (
          <li {...props} key={opt.ticker}>
            <Box sx={{ display: "flex", justifyContent: "space-between", width: "100%" }}>
              <span>{opt.ticker}</span>
              {"qtyRemain" in opt ? (
                <span style={{ opacity: 0.7 }}>{(opt as any).qtyRemain}</span>
              ) : null}
            </Box>
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Ticker"
            size="small"
            placeholder="MSFT"
            inputProps={{ ...params.inputProps, maxLength: 10 }}
          />
        )}
        sx={{ minWidth: 220 }}
      />

      <TextField
        label="Qty"
        size="small"
        type="number"
        value={qty}
        onChange={(e) => setQty(Number(e.target.value) || 0)}
        inputProps={{ min: 0, step: 1 }}
      />

      <TextField
        label="Price"
        size="small"
        type="number"
        value={price}
        onChange={(e) => setPrice(Number(e.target.value) || 0)}
        inputProps={{ min: 0, step: "0.01" }}
      />

      <TextField
        label="Date"
        size="small"
        type="date"
        value={date}
        onChange={(e) => setDate(e.target.value)}
        InputLabelProps={{ shrink: true }}
      />

      <Button variant="contained" disabled={!canBuy} onClick={handleBuy}>
        {buy.isPending ? "Buying…" : "Buy"}
      </Button>

      {buy.isError && (
        <Alert severity="error" sx={{ ml: 1 }}>
          Ошибка покупки
        </Alert>
      )}

      {willCreate && (
        <Alert severity="info" variant="outlined" icon={false} sx={{ py: 0.5, px: 1 }}>
          New ticker will be created
        </Alert>
      )}
    </Stack>
  );
}
