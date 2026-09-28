
import AppDataGrid from "./AppDataGrid";
import { LoadError } from "./LoadError";
import type { GridColDef } from '@mui/x-data-grid';
import { useLots, type Lot } from '../hooks/useTrades';

export function LotsGrid() {
  const { data = [], isLoading, error, refetch } = useLots();

  if (isLoading) return <p>Loading lots…</p>;
  if (error) return <LoadError what="lots" onRetry={() => refetch()} />;
  if (!data || data.length === 0) return <p>(no lots yet)</p>;

  const rows: Lot[] = data;

  const columns: GridColDef<Lot>[] = [
    { field: 'ticker', 
      headerName: 'Ticker',
      flex: 1,
    },

    { field: 'purchaseDate',
      headerName: 'Purchase Date',
      flex: 1,
      valueFormatter: (value: string) => value.slice(0, 10),
    },
    {
      field: 'qtyInitial',
      headerName: 'Qty (initial)',
      type: 'number',
      flex: 1,
    },
    {
      field: 'qtyRemain',
      headerName: 'Qty (remain)',
      type: 'number',
      flex: 1,
    },
    {
      field: 'unitCost',
      headerName: 'Unit cost',
      type: 'number',
      flex: 1,
    },
  ];

  return (
    <div style={{ width: '100%' }}>
      <AppDataGrid
        rows={rows}
        columns={columns}
        getRowId={(row) => row.id}
      />
    </div>
  );
}
