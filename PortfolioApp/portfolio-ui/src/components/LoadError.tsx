import { Alert, Button } from "@mui/material";

export function LoadError({ what, onRetry }: { what: string; onRetry: () => void }) {
  return (
    <Alert
      severity="error"
      action={<Button color="inherit" size="small" onClick={onRetry}>Retry</Button>}
    >
      Failed to load {what}
    </Alert>
  );
}
