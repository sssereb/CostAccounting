// src/components/LotsGrid.tsx
import { useLots } from "../hooks/useTrades";
import { format }  from "date-fns";

export function LotsGrid() {
  const { data = [], isLoading, error, refetch } = useLots();

  if (isLoading) return <p>Loading lots…</p>;
  if (error)     return (
    <p className="text-red-500">
      Failed to load lots —
      <button onClick={() => refetch()} className="underline ml-1">retry</button>
    </p>
  );

  if (data.length === 0) return <p>(no lots yet)</p>;

  return (
    <section className="p-4 border rounded min-w-[320px]">
      <h3 className="mb-2">Lots</h3>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead className="bg-gray-700">
            <tr className="text-left text-gray-200">
              <th className="px-2 py-1">Ticker</th>
              <th className="px-2 py-1">Date</th>
              <th className="px-2 py-1 text-right">Qty</th>
              <th className="px-2 py-1 text-right">Unit cost</th>
            </tr>
          </thead>

          <tbody>
            {data.map((lot, i) => (
              <tr
                key={i}
                className={i % 2 ? "bg-gray-800" : "bg-gray-900"}
              >
                <td className="px-2 py-1">{lot.ticker}</td>
                <td className="px-2 py-1">
                  {format(new Date(lot.purchaseDate), "yyyy-MM-dd")}
                </td>
                <td className="px-2 py-1 text-right">{lot.qtyRemain}</td>
                <td className="px-2 py-1 text-right">
                  {lot.unitCost.toFixed(2)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
