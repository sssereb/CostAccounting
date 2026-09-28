// src/components/SellForm.tsx
import { useState } from "react";
import {
    Stack, TextField, Button, FormControl, InputLabel, Select, MenuItem,
    Autocomplete, Box, Alert
} from "@mui/material";
import { useSell, useAssetId, useAssets, type Asset } from "../hooks/useTrades";
import type { SellRequestDto } from "../hooks/useTrades";
import { useDebounce } from "../utils/useDebounce";
import { getErrorMessage } from "../utils/useErrors";

const todayIsoDate = () => new Date().toISOString().slice(0, 10);
const isoFromDateInput = (yyyyMmDd: string) => `${yyyyMmDd}T00:00:00`;

export default function SellForm() {
    const [ticker, setTicker] = useState("");
    const [qty, setQty] = useState<number>(0);
    const [price, setPrice] = useState<number>(0);
    const [method, setMethod] = useState<SellRequestDto["method"]>("FIFO");
    const [date, setDate] = useState<string>(todayIsoDate());

    const assetsQ = useAssets();
    const assets = (assetsQ.data ?? []) as Asset[];

    // resolve the assetId for the typed ticker
    const debouncedTicker = useDebounce(ticker.trim().toUpperCase(), 300);
    const assetIdQ = useAssetId(debouncedTicker);
    const assetId = assetIdQ.data ?? ""; // always a string

    const sell = useSell();

    const canSell =
        assetId !== "" && qty > 0 && price > 0 && !sell.isPending;

    const handleSell = () => {
        if (!canSell) return;
        const dto: SellRequestDto = {
            assetId,
            qty,
            price,
            date: isoFromDateInput(date),
            method,
        };
        sell.mutate(dto);
    };

    const selectedAsset =
        assets.find(a => a.ticker.toUpperCase() === ticker.toUpperCase()) ?? null;

    const showNotFound =
        ticker.length > 0 && !assetsQ.isLoading && assetId === "";

    return (
        <Stack direction="row" spacing={2} alignItems="center" useFlexGap flexWrap="wrap">
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
                                <span style={{ opacity: 0.7 }}>{opt.qtyRemaining}</span>
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
                        error={showNotFound}
                        helperText={showNotFound ? "Asset not found for this ticker" : undefined}
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
                inputProps={{ min: 1, step: 1 }}
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

            <Button variant="contained" disabled={!canSell} onClick={handleSell}>
                {sell.isPending ? "Selling…" : "Sell"}
            </Button>

            <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel id="sell-method-label">Method</InputLabel>
                <Select
                    labelId="sell-method-label"
                    label="Method"
                    value={method}
                    onChange={(e) => setMethod((e.target.value as SellRequestDto["method"]) ?? "FIFO")}
                >
                    <MenuItem value="FIFO">FIFO</MenuItem>
                    <MenuItem value="LIFO">LIFO</MenuItem>
                    <MenuItem value="AVG">AVG</MenuItem>
                </Select>
            </FormControl>
            
            {sell.isError && (
  <Alert severity="error" sx={{ ml: 1 }}>
    {getErrorMessage(sell.error)}
  </Alert>
)}

        </Stack>
    );
}
