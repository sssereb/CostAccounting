import {
  DataGrid,
  type DataGridProps,
  type GridValidRowModel,
} from "@mui/x-data-grid";

export type AppDataGridProps<R extends GridValidRowModel = GridValidRowModel> = Omit<
  DataGridProps<R>,
  "slots" | "slotProps" | "density" | "disableRowSelectionOnClick" | "autoHeight"
> & {  fixedHeight?: number;
};

export default function AppDataGrid<R extends GridValidRowModel = GridValidRowModel>(
  props: AppDataGridProps<R>
) {
  const {
    fixedHeight,
    pageSizeOptions,
    initialState,
    sx,
    ...rest
  } = props;

  const commonSx = {
    width: "60vw",
    "& .MuiDataGrid-columnHeaders .MuiDataGrid-columnHeaderTitle": { fontWeight: 600 },
    "& .MuiDataGrid-cell": { fontSize: 14 },
    ...sx,
  } as const;

  const grid = (
    <DataGrid<R>
      density="compact"
      disableRowSelectionOnClick
      slotProps={{
        toolbar: { showQuickFilter: true, quickFilterProps: { debounceMs: 300 }},
      }}

      pageSizeOptions={pageSizeOptions ?? [10, 25, 50, 100]}
      initialState={
        initialState ?? {
          pagination: { paginationModel: { pageSize: 10, page: 0 } },
        }
      }
      sx={commonSx}
      {...rest}
    />
  );

  if (fixedHeight && fixedHeight > 0) {
    return (
      <div style={{ width: "100%", height: fixedHeight }}>
        {grid}
      </div>
    );
  }

  return grid;
}

