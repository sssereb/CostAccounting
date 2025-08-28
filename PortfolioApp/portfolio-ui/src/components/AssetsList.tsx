// src/components/AssetsList.tsx
import { useAssets, type Asset } from "../hooks/useTrades";
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';



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

  const rows = data as Asset[];

  const columns: GridColDef<Asset>[] = [
    { field: 'ticker', 
      headerName: 'Ticker', 
      flex: 1 
    },

    { field: 'id', 
      headerName: 'Guid', 
      flex: 1 
    }
  ];

  return (
    <div style={{ display: 'flex', flex: 1, width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        getRowId={(row) => row.id ?? `${row.ticker}` }
        initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        pageSizeOptions={[5, 10, 25]}
        style={{ flex: 1 }}
      />
    </div>
  );
}

