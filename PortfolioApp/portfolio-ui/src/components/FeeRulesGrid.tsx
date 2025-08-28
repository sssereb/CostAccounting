import { useFeeRules } from '../hooks/useFeeRules';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';

/* ---- DTO ---- */
type FeeType = "FixedPerTrade" | "FixedPerShare" | "PercentOfValue";
type FeeDirection = "Buy" | "Sell" | "Both";
interface FeeRuleDto {
  type: FeeType;
  amount: number;
  direction: FeeDirection;
}



export default function FeeRulesGrid() {
  const { data = [], isLoading, error, refetch } = useFeeRules();

  if (isLoading) return <p>Loading fee rules</p>;
  if (error)
    return (
      <p className="text-red-500">
        Failed to load fee rules —
        <button onClick={() => refetch()} className="underline ml-1">
          retry
        </button>
      </p>
    );
  if (!data || data.length === 0) return <p>(no fee rules yet)</p>;

  const rows = data as FeeRuleDto[];

  const columns: GridColDef<FeeRuleDto>[] = [
    { field: 'type', 
      headerName: 'Type',
      type: 'string', 
      flex: 1 
    },

    { field: 'amount', 
      headerName: 'Amount', 
      flex: 1 
    },
    {
      field: 'direction',
      headerName: 'Direction',
      type: 'string',
      flex: 1 
    }
  ];

  return (
    <div style={{ display: 'flex', flex: 1, width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        getRowId={(row) => `${row.type}-${row.amount}-${row.direction}`}
        style={{ flex: 1 }}
      />
    </div>
  );
}

