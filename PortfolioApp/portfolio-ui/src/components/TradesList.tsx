import { useTrades, type Trade } from "../hooks/useTrades";
import AppDataGrid from "./AppDataGrid";
import { LoadError } from "./LoadError";
import type { GridColDef } from '@mui/x-data-grid';


export function TradesList() {
  const { data = [], isLoading, error, refetch } = useTrades();

  if (isLoading) return <p>Loading trades</p>;
  if (error) return <LoadError what="trades" onRetry={() => refetch()} />;
  if (!data || data.length === 0) return <p>(no trades yet)</p>;

  const rows: Trade[] = data;

  const columns: GridColDef<Trade>[] = [
    { field: 'ticker', 
      headerName: 'Ticker',
      flex: 1,
    },
    { field: 'date',
      headerName: 'Date',
      flex: 1,
      valueFormatter: (value: string) => value.slice(0, 10),
    },
    {
      field: 'quantity',
      headerName: 'Quantity',
      type: 'number',
      flex: 1,
    },
    {
      field: 'price',
      headerName: 'Price',
      type: 'number',
      flex: 1,
    },
    {
      field: 'profitGross',
      headerName: 'Gross profit',
      description: 'Proceeds minus purchase price, without any fees',
      type: 'number',
      flex: 1,
    },
    {
      field: 'profitNet',
      headerName: 'Net profit',
      description: 'Proceeds minus cost basis (buy fees included) minus sell fees',
      type: 'number',
      flex: 1,
    }
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

