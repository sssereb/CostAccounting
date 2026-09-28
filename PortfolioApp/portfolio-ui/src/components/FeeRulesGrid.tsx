import { useFeeRules, type FeeRuleDto } from '../hooks/useFeeRules';
import AppDataGrid from "./AppDataGrid";
import type { GridColDef } from '@mui/x-data-grid';

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

  const rows: FeeRuleDto[] = data;

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
    <div style={{ width: '100%' }}>
      <AppDataGrid
        rows={rows}
        columns={columns}
        getRowId={(row) => `${row.type}-${row.amount}-${row.direction}`}
      />
    </div>
  );
}

