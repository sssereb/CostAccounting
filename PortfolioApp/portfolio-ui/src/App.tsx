import PortfolioTabs     from "./components/Tabs";
import { createTheme, ThemeProvider } from '@mui/material/styles';

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
  },
});

export default function App() {
  return (
    <ThemeProvider theme={darkTheme}>
      <PortfolioTabs />
    </ThemeProvider>
  );
}
