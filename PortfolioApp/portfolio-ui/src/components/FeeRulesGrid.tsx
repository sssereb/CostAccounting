// src/components/FeeRulesReadOnly.tsx
//
// • Один GET  /fees/get   при монтировании.
// • Никаких PUT/POST/DELETE, кнопок и «грязных» состояний.
// • Если список пуст — показываем «(no rules)».
// • Tailwind-классы только для минимальной читабельности.
//
import { useEffect, useState } from "react";
import { http } from "../lib/http";

/* ---- DTO ---- */
type FeeType = "FixedPerTrade" | "FixedPerShare" | "PercentOfValue";
type FeeDirection = "Buy" | "Sell" | "Both";
interface FeeRuleDto {
  type: FeeType;
  amount: number;
  direction: FeeDirection;
}

export default function FeeRulesGrid() {
  const [rules, setRules]   = useState<FeeRuleDto[] | null>(null); // null = loading
  const [error, setError]   = useState<string | null>(null);

  useEffect(() => {
    http.get<FeeRuleDto[]>("/fees/get")
      .then(r => setRules(r.data))
      .catch(e => {
        setError(e.message || "Request failed");
        setRules([]);                 // чтобы отрендерить таблицу
      });
  }, []);

  /* ---------- UI ---------- */
  if (rules === null) return <p className="p-4">Loading…</p>;

  return (
    <div className="max-w-xl mx-auto mt-8 text-sm">
      <h2 className="text-xl font-semibold mb-4">Fees Rules</h2>

      {error && (
        <div className="mb-3 text-red-600 border p-2 rounded">{error}</div>
      )}

      {rules.length === 0 ? (
        <p className="p-3 text-gray-500 italic">(no rules)</p>
      ) : (
        <table className="w-full border rounded">
          <thead className="bg-gray-100 text-left">
            <tr>
              <th className="p-2">Fee type</th>
              <th className="p-2">Amount</th>
              <th className="p-2">Direction</th>
            </tr>
          </thead>
          <tbody>
            {rules.map((r, idx) => (
              <tr key={idx} className="even:bg-gray-50">
                <td className="p-2">{r.type}</td>
                <td className="p-2">{r.amount}</td>
                <td className="p-2">{r.direction}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
