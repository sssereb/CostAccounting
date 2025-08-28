import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import { format } from 'date-fns';
import { useLots } from '../hooks/useTrades';

// --- safe date helpers ---
function toDateSafe(d: string | number | Date | null) {
  if (d == null) return null;
  const dt = d instanceof Date ? d : new Date(d);
  return isNaN(dt.getTime()) ? null : dt;
}
function formatYMD(d: string | number | Date | null) {
  const dt = toDateSafe(d);
  return dt ? format(dt, 'yyyy-MM-dd') : '';
}

type Lot = {
  id?: string | number;
  ticker: string;
  purchaseDate: string | Date;
  qtyInitial: number;
  qtyRemain: number;
  unitCost: number;
};

export function LotsGrid() {
  const { data = [], isLoading, error, refetch } = useLots();

  if (isLoading) return <p>Loading lots…</p>;
  if (error)
    return (
      <p className="text-red-500">
        Failed to load lots —
        <button onClick={() => refetch()} className="underline ml-1">
          retry
        </button>
      </p>
    );
  if (!data || data.length === 0) return <p>(no lots yet)</p>;

  const rows = data as Lot[];

  const columns: GridColDef<Lot>[] = [
    { field: 'ticker', 
      headerName: 'Ticker', 
      flex: 1 
    },

    { field: 'purchaseDate', 
      headerName: 'Purchase Date', 
      flex: 1 
    },
    {
      field: 'qtyInitial',
      headerName: 'Qty (initial)',
      type: 'number',
      flex: 1 
    },
    {
      field: 'qtyRemain',
      headerName: 'Qty (remain)',
      type: 'number',
      flex: 1 
    },
    {
      field: 'unitCost',
      headerName: 'Unit cost',
      type: 'number',
      flex: 1 
    },
  ];

  return (
    <div style={{ display: 'flex', flex: 1, width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        getRowId={(row) => row.id ?? `${row.ticker}-${formatYMD(row.purchaseDate)}-${row.qtyRemain}-${row.unitCost}` }
        initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        pageSizeOptions={[5, 10, 25]}
        style={{ flex: 1 }}
      />
    </div>
  );
}
