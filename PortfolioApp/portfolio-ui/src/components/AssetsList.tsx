// src/components/AssetsList.tsx
import { useAssets, type Asset } from "../hooks/useTrades";
import type { GridColDef } from '@mui/x-data-grid';
import AppDataGrid from "./AppDataGrid";



export function AssetsList() {
  const { data = [], isLoading, error, refetch } = useAssets();

  if (isLoading) return <p>Loading assets</p>;
  if (error)
    return (
      <p className="text-red-500">
        Failed to load assets —
        <button onClick={() => refetch()} className="underline ml-1">
          retry
        </button>
      </p>
    );
  if (!data || data.length === 0) return <p>(no assets yet)</p>;

  const rows: Asset[] = data;

  const columns: GridColDef<Asset>[] = [
    { field: 'ticker', 
      headerName: 'Ticker',
      flex: 1,
    },

    { field: 'qtyRemaining', 
      headerName: 'Quantity (remaining)',
      flex: 1,
    },

    { field: 'lastPrice', 
      headerName: 'Last Price',
      flex: 1,
    }
  ];

  return (
    <div style={{ width: '100%' }}>
      <AppDataGrid
        rows={rows}
        columns={columns}
        getRowId={(row) => `${row.ticker}-${row.qtyRemaining}-${row.lastPrice}` }
      />
    </div>
  );
}

