// src/pages/PortfolioTabs.tsx
import React, { useEffect, useState } from "react";

import { AssetsList }   from "../components/AssetsList";
import { TradesList }   from "../components/TradesList";
import { LotsGrid }     from "../components/LotsGrid";
import { BuyForm }      from "../components/BuyForm";
import { SellForm }     from "../components/SellForm";
// FeeRulesGrid экспортируется по умолчанию (судя по импорту без фигурных скобок)
import FeeRulesGrid     from "../components/FeeRulesGrid";

/** --- helpers: читать/писать активную вкладку в URL --- */
function readTabFromLocation(queryKey: string, fallback: string) {
  try {
    const url = new URL(window.location.href);
    const fromQuery = url.searchParams.get(queryKey);
    if (fromQuery) return fromQuery;

    // Фоллбек для sandbox/iframes: #tab=...
    const hash = window.location.hash.slice(1);
    const [k, v] = hash.split("=");
    if (k === queryKey && v) return decodeURIComponent(v);
  } catch {}
  return fallback;
}

function writeTabToLocation(queryKey: string, value: string) {
  try {
    const url = new URL(window.location.href);
    url.searchParams.set(queryKey, value);
    const proto = window.location.protocol;
    if (proto === "http:" || proto === "https:") {
      window.history.replaceState(window.history.state, "", url);
    } else {
      // about:srcdoc / file: — используем hash
      window.location.hash = `${queryKey}=${encodeURIComponent(value)}`;
    }
  } catch {}
}

/** --- Tabs URL-only (без контролируемого режима и клавиш) --- */
type TabItem = {
  id: string;
  label: string;
  content: React.ReactNode | (() => React.ReactNode);
};

function TabsUrl({ items, queryKey = "tab", initialId }: {
  items: TabItem[];
  queryKey?: string;
  initialId?: string;
}) {
  const firstId = initialId ?? items?.[0]?.id ?? "";
  const [active, setActive] = useState<string>(() =>
    readTabFromLocation(queryKey, firstId)
  );

  useEffect(() => {
    if (active) writeTabToLocation(queryKey, active);
  }, [active, queryKey]);

  return (
    <div className="w-full">
      <div className="flex gap-2 border-b px-2" role="tablist" aria-label="Tabs">
        {items.map((t) => {
          const selected = t.id === active;
          return (
            <button
              key={t.id}
              onClick={() => setActive(t.id)}
              className={
                "px-4 py-2 rounded-t-2xl " +
                (selected ? "bg-gray-100 border-x border-t" : "opacity-70 hover:opacity-100")
              }
            >
              {t.label}
            </button>
          );
        })}
      </div>

      {items.map((t) => (
        <div
          key={t.id}
          hidden={t.id !== active}
          className="border-x border-b rounded-b-2xl p-4"
          role="tabpanel"
        >
          {typeof t.content === "function" ? (t.content as () => React.ReactNode)() : t.content}
        </div>
      ))}
    </div>
  );
}

/** --- Страница с тремя вкладками --- */
export default function PortfolioTabs() {
  const items: TabItem[] = [
    {
      id: "trading",
      label: "Trading",
      content: (
        <section className="space-y-4">
          <h2 className="text-xl font-semibold">Sell / Buy</h2>
              <BuyForm />
              <SellForm />
          <h2 className="text-xl font-semibold">Lots</h2>
          <LotsGrid />
        </section>
      ),
    },
    {
      id: "trade",
      label: "Trades",
      content: (
        <section className="space-y-4">
          <h2 className="text-xl font-semibold">Trade (History)</h2>
          <TradesList />
        </section>
      ),
    },
    {
      id: "others",
      label: "Others",
      content: (
        <section className="space-y-4">
          <div>
            <h2 className="text-xl font-semibold">Assets</h2>
            <AssetsList />
          </div>
          <div>
            <h2 className="text-xl font-semibold">Fee Rules</h2>
            <FeeRulesGrid />
          </div>
        </section>
      ),
    },
  ];

  return (
    <main className="max-w-5xl mx-auto py-6">
      <TabsUrl items={items} queryKey="tab" initialId="trade" />
    </main>
  );
}
