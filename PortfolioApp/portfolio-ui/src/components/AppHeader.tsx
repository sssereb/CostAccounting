import { AppBar, Toolbar, Box, Typography } from '@mui/material';

export default function AppHeader() {
  return (
    <AppBar position="static" color="transparent" elevation={0}>
      <Toolbar sx={{ gap: 1.5 }}>
        <Box component="img" src="/logo-icon.svg" alt="Portfolio" sx={{ height: 50 }} />
        <Typography variant="h6" sx={{ fontWeight: 700 }}>Portfolio Accounting</Typography>
      </Toolbar>
    </AppBar>
  );
}
