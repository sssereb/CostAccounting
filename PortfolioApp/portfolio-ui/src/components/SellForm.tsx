import { useState } from "react";
import {
    Stack, TextField, Button, FormControl, InputLabel, Select, MenuItem,
    Autocomplete, Box, Alert
} from "@mui/material";
import { useSell, useAssetId, useAssets, type Asset, type CostBasisMethod, type SellRequestDto } from "../hooks/useTrades";
import { useDebounce } from "../utils/useDebounce";
import { getErrorMessage } from "../utils/useErrors";

const todayIsoDate = () => new Date().toISOString().slice(0, 10);
const isoFromDateInput = (yyyyMmDd: string) => `${yyyyMmDd}T00:00:00`;

// Keyed by the generated enum: a value the API does not accept, or a missing one, fails tsc.
const METHOD_LABELS = { FIFO: "FIFO", LIFO: "LIFO", Average: "AVG" } satisfies Record<CostBasisMethod, string>;

export default function SellForm() {
    const [ticker, setTicker] = useState("");
    const [qty, setQty] = useState<number>(0);
    const [price, setPrice] = useState<number>(0);
    const [method, setMethod] = useState<CostBasisMethod>("FIFO");
    const [date, setDate] = useState<string>(todayIsoDate());

    const assetsQ = useAssets();
    const assets: Asset[] = assetsQ.data ?? [];

    // The request carries the ticker as typed. The lookup only enables the button once the
    // asset is known for exactly this ticker, so a result for a previous ticker is never used.
    const normalizedTicker = ticker.trim().toUpperCase();
    const debouncedTicker = useDebounce(normalizedTicker, 300);
    const assetIdQ = useAssetId(debouncedTicker);
    const lookupIsCurrent = debouncedTicker === normalizedTicker && !assetIdQ.isFetching;
    const assetKnown = lookupIsCurrent && !!assetIdQ.data;

    const sell = useSell();

    const canSell =
        assetKnown && qty > 0 && price > 0 && !sell.isPending;

    const handleSell = () => {
        if (!canSell) return;
        const dto: SellRequestDto = {
            ticker: normalizedTicker,
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
        normalizedTicker.length > 0 && lookupIsCurrent && !assetIdQ.data;

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
                    onChange={(e) => setMethod(e.target.value as CostBasisMethod)}
                >
                    {Object.entries(METHOD_LABELS).map(([value, label]) => (
                        <MenuItem key={value} value={value}>{label}</MenuItem>
                    ))}
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
