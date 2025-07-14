import { useState } from "react";
import { useAssetId, useSell } from "../hooks/useTrades";
import type { SellRequestDto } from "../hooks/useTrades";
import { useDebounce } from "../hooks/useDebounce";         // если вынесен отдельно

const todayIsoDate = () => new Date().toISOString().slice(0, 10);

export function SellForm() {
  /* --- локальное состояние --- */
  const [ticker, setTicker]   = useState("");
  const [qty, setQty]         = useState(0);
  const [price, setPrice]     = useState(0);
  const [method, setMethod]   = useState<SellRequestDto["method"]>("LIFO");
  const [date, setDate] = useState(todayIsoDate());

  /* --- поиск assetId --- */
  const assetIdQ = useAssetId(useDebounce(ticker.trim().toUpperCase(), 400));
  /* --- отправка продажи --- */
  const sell = useSell();

  const canSell =
    assetIdQ.isSuccess && qty > 0 && price > 0 && !sell.isPending;

  const handleSell = () => {
    if (!assetIdQ.data) return;

    sell.mutate({
      assetId: assetIdQ.data,
      qty,
      price,
      date: new Date().toISOString(),
      method
    });
  };

  return (
    <section className="p-4 border rounded" style={{ minWidth: 650 }}>
      <h3>Sell asset</h3>

      <div style={{ display: "flex", gap: 8 }}>
        <input
          placeholder="Ticker"
          value={ticker}
          onChange={e => setTicker(e.target.value.toUpperCase())}
        />

        <input
          type="number"
          placeholder="Qty"
          value={qty}
          onChange={e => setQty(+e.target.value)}
        />

        <input
          type="number"
          placeholder="Price"
          value={price}
          onChange={e => setPrice(+e.target.value)}
        />

        <input
          type="date"
          value={date}
          onChange={(e) => setDate(e.target.value)}
        />

        <select value={method} onChange={e => setMethod(e.target.value as any)}>
          <option value="FIFO">FIFO</option>
          <option value="LIFO">LIFO</option>
          <option value="AverageCost">AverageCost</option>
        </select>

        <button onClick={handleSell} disabled={!canSell}>
          {sell.isPending ? "Sending…" : "Sell"}
        </button>
      </div>

      {assetIdQ.isLoading && ticker && <p>🔍 looking up asset…</p>}
      {assetIdQ.isError   && <p style={{ color: "orange" }}>No such ticker</p>}

      {sell.isSuccess && (
        <pre style={{ color: "green" }}>
          ✅ success:&nbsp;{JSON.stringify(sell.data, null, 2)}
        </pre>
      )}

      {sell.isError && (
        <pre style={{ color: "red" }}>
          {JSON.stringify((sell.error as any).response?.data, null, 2)}
        </pre>
      )}
    </section>
  );
}
