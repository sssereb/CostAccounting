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
} from "../hooks/useTrades";
import { useDebounce } from "../utils/useDebounce";
import { getErrorMessage } from "../utils/useErrors";

const todayIsoDate = () => new Date().toISOString().slice(0, 10); // "YYYY-MM-DD"
const isoFromDateInput = (yyyyMmDd: string) => `${yyyyMmDd}T00:00:00`;

export default function BuyForm() {
  const [ticker, setTicker] = useState("");
  const [qty, setQty] = useState<number>(0);
  const [price, setPrice] = useState<number>(0);
  const [date, setDate] = useState<string>(todayIsoDate());

  // all assets for the dropdown
  const assetsQ = useAssets();
  const assets: Asset[] = assetsQ.data ?? [];

  // The request always carries the ticker as typed; the debounced lookup only drives the "new ticker" hint.
  const normalizedTicker = ticker.trim().toUpperCase();
  const debouncedTicker = useDebounce(normalizedTicker, 300);
  const assetIdQ = useAssetId(debouncedTicker);
  const buy = useBuy();

  const canBuy =
    normalizedTicker.length > 0 && qty > 0 && price > 0 && !buy.isPending;

  const handleBuy = () => {
    if (!canBuy) return;
    buy.mutate({ ticker: normalizedTicker, qty, price, date: isoFromDateInput(date) });
  };

  // currently selected Autocomplete value (object)
  const selectedAsset =
    assets.find((a) => a.ticker.toUpperCase() === ticker.toUpperCase()) ?? null;

  const willCreate =
    debouncedTicker === normalizedTicker && !!debouncedTicker && !assetIdQ.isFetching && !assetIdQ.data;

  return (
    <Stack direction="row" spacing={2} alignItems="center" useFlexGap flexWrap="wrap">
      {/* Ticker: dropdown plus free text input */}
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
              <span style={{ opacity: 0.7 }}>{opt.qtyRemaining}</span>
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
          {getErrorMessage(buy.error)}
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
