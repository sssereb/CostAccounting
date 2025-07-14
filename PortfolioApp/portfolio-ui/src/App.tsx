// src/App.tsx
import { AssetsList }  from "./components/AssetsList";
import { LotsGrid }  from "./components/LotsGrid";
import { BuyForm }     from "./components/BuyForm";
import { SellForm }    from "./components/SellForm";

export default function App() {
  return (
    <main className="flex flex-col gap-6 p-8">
      {/* ─── верх: два столбца ─── */}
      <div className="grid md:grid-cols-2 gap-6">
        <AssetsList />
        <LotsGrid />
      </div>

      {/* ─── низ: формы одна под другой ─── */}
      <BuyForm />
      <SellForm />
    </main>
  );
}
