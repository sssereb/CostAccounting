// src/components/AssetsList.tsx
import { useAssets } from "../hooks/useTrades";

export function AssetsList() {
  const { data, isLoading, error } = useAssets();

  if (isLoading) return <p>Loading assets…</p>;
  if (error)     return <p style={{color:"red"}}>{String(error)}</p>;

  return (
    <section className="p-4 border rounded" style={{minWidth: 300}}>
      <h3>Assets</h3>
      <table border={1} cellPadding={4}>
        <thead><tr><th>Ticker</th><th>Id</th></tr></thead>
        <tbody>
          {data!.map(a => (
            <tr key={a.id}>
              <td>{a.ticker}</td>
              <td>{a.id}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
