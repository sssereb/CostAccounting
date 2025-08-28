import PortfolioTabs from "./components/Tabs";
import { createTheme, ThemeProvider, CssBaseline, Container } from '@mui/material';


const darkTheme = createTheme({
  palette: { mode: 'dark' },
});

export default function App() {
  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline />
      <Container maxWidth={false} sx={{ py: 2 }}>
        
      <PortfolioTabs />
      </Container>
    </ThemeProvider>
  );
}
