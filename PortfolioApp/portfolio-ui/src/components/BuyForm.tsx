import { useState } from "react";
import { useAssetId, useBuy } from "../hooks/useTrades";

// helper: yyyy-MM-dd string for today
const todayIsoDate = () => new Date().toISOString().slice(0, 10);

export function BuyForm() {
  const [ticker, setTicker] = useState("");
  const [qty, setQty] = useState<number>(0);
  const [price, setPrice] = useState<number>(0);
  const [date, setDate] = useState(todayIsoDate());

  const assetIdQ = useAssetId(ticker.trim().toUpperCase());
  const buy = useBuy();

  const canBuy = ticker.trim() !== "" && qty > 0 && price > 0 && !buy.isPending;

  const handleBuy = () => {
    if (!canBuy) return;
    buy.mutate(
      {
        ticker: ticker.trim().toUpperCase(),
        assetId: assetIdQ.isSuccess ? assetIdQ.data : undefined,
        qty,
        price,
        date: new Date(date).toISOString(),
      },
      {
        onSuccess: () => {
          setQty(0);
          setPrice(0);
        },
      }
    );
  };

  return (
    <section className="p-4 border rounded" style={{ minWidth: 650 }}>
      <h3 className="mb-2">Buy asset</h3>

      {/* ── одна строка, без переноса ── */}
      <div style={{ display: "flex", gap: 8 }}>
          <input
            value={ticker}
            onChange={(e) => setTicker(e.target.value.toUpperCase())}
            placeholder="Ticker"
          />
          

          <input
            type="number"
            value={qty}
            onChange={(e) => setQty(+e.target.value)}
            min={1}
          />

          <input
            type="number"
            value={price}
            onChange={(e) => setPrice(+e.target.value)}
            min={0.01}
            step="0.01"
          />

          <input
            type="date"
            value={date}
            onChange={(e) => setDate(e.target.value)}
          />

        <button
          onClick={handleBuy}
          disabled={!canBuy}
          className={`px-4 py-2 h-[38px] rounded self-end shrink-0 ${canBuy ? "bg-indigo-600 hover:bg-indigo-700" : "bg-gray-600 cursor-not-allowed"}`}
        >
          {buy.isPending ? "Saving…" : "Buy"}
        </button>
      </div>

      {/* подсказки */}
      <div className="mt-2 text-sm">
        {assetIdQ.isLoading && ticker && <p>🔍 checking ticker…</p>}
        {assetIdQ.isSuccess && <p className="text-green-400">Ticker exists</p>}
        {assetIdQ.isError && ticker && (
          <p className="text-yellow-400">New ticker will be created</p>
        )}
        {buy.isSuccess && <p className="text-green-400">✓ Buy recorded</p>}
        {buy.isError && (
          <pre className="text-red-400 whitespace-pre-wrap">
            {JSON.stringify((buy.error as any).response?.data ?? buy.error, null, 2)}
          </pre>
        )}
      </div>
    </section>
  );
}
