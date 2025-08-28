// src/pages/PortfolioTabs.tsx
import { useEffect, useState } from "react";
import {
  AppBar, Toolbar, Box, Paper, Typography, Divider, Tabs, Tab,
} from "@mui/material";

import { AssetsList } from "../components/AssetsList";
import { TradesList } from "../components/TradesList";
import { LotsGrid } from "../components/LotsGrid";
import BuyForm from "../components/BuyForm";
import SellForm from "../components/SellForm";
import FeeRulesGrid from "../components/FeeRulesGrid";

/* URL helpers */
function readTabFromLocation(queryKey: string, fallback: string) {
  try {
    const url = new URL(window.location.href);
    const q = url.searchParams.get(queryKey);
    if (q) return q;
    const hash = window.location.hash.slice(1);
    const [k, v] = hash.split("=");
    if (k === queryKey && v) return decodeURIComponent(v);
  } catch { }
  return fallback;
}

function writeTabToLocation(queryKey: string, value: string) {
  try {
    const url = new URL(window.location.href);
    url.searchParams.set(queryKey, value);
    const proto = window.location.protocol;
    if (proto === "http:" || proto === "https:")
      window.history.replaceState(window.history.state, "", url);
    else
      window.location.hash = `${queryKey}=${encodeURIComponent(value)}`;
  } catch { }
}

/* тип вкладки */
type TabItem = { id: string; label: string; content: React.ReactNode | (() => React.ReactNode) };

export default function PortfolioTabs() {
  const items: TabItem[] = [
    {
      id: "trading",
      label: "Trading",
      content: (
        <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>

          <Typography variant="h5" sx={{ mb: 2 }}>Sell / Buy</Typography>
            <Typography variant="subtitle1" sx={{ mb: 1 }}>Buy</Typography>
          
            <BuyForm />
          
            <Divider sx={{ my: 2 }} />
          
            <Typography variant="subtitle1" sx={{ mb: 1 }}>Sell</Typography>
          
            <SellForm />
          
          <Divider sx={{ my: 2 }} />
          
          <Typography variant="h5" sx={{ mb: 2 }}>Lots</Typography>
          
          <LotsGrid />
        </Paper>
      ),
    },
    {
      id: "trade",
      label: "Trades",
      content: (
        <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
          <Typography variant="h5" sx={{ mb: 2 }}>Trade (History)</Typography>
          <TradesList />
        </Paper>
      ),
    },
    {
      id: "others",
      label: "Others",
      content: (
        <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
          <Typography variant="h5" sx={{ mb: 2 }}>Fee Rules</Typography>
          <FeeRulesGrid />
          <Divider sx={{ my: 2 }} />
          <Typography variant="h5" sx={{ mb: 2 }}>Assets</Typography>
          <AssetsList />
        </Paper>
      ),
    },
  ];

  const firstId = "trade";
  const [active, setActive] = useState<string>(() => readTabFromLocation("tab", firstId));
  useEffect(() => { if (active) writeTabToLocation("tab", active); }, [active]);

  return (
    <Box sx={{ width: "100%" }}>
      <AppBar position="sticky" color="transparent" elevation={0}>
        <Toolbar sx={{ px: 0 }}>
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 2,
              width: "100%",
              px: 1.5,
              py: 0.75,
              borderRadius: 2,
              bgcolor: "rgba(255,255,255,0.04)", 
            }}
          >
            {/* Logo */}
            <Box component="img" src="/logo-horizontal.svg" alt="Portfolio" sx={{ height: 64 }} />

            {/* Tabs */}
            <Box
              sx={{
                flex: 1,
                minWidth: 0,
                "& .MuiTabs-indicator": { display: "none" },
                "& .MuiTab-root": {
                  textTransform: "none",
                  fontWeight: 600,
                  borderRadius: 999,
                  m: 0.5, px: 1.75, py: 0.75,
                  color: "rgba(255,255,255,0.7)",
                  transition: "all .18s ease",
                  "&:hover": {
                    bgcolor: "rgba(255,255,255,0.08)",
                    color: "#fff",
                    transform: "translateY(-1px)",
                  },
                  "&.Mui-selected": {
                    bgcolor:
                      "linear-gradient(180deg, rgba(255,255,255,.16), rgba(255,255,255,.08))",
                    color: "#fff",
                    boxShadow:
                      "0 4px 10px rgba(0,0,0,.25), inset 0 1px 0 rgba(255,255,255,.12)",
                  },
                },
              }}
            >
              <Tabs value={active} onChange={(_, v) => setActive(v)} variant="scrollable" scrollButtons="auto">
                {items.map(t => (
                  <Tab key={t.id} value={t.id} label={t.label} id={`tab-${t.id}`} />
                ))}
              </Tabs>
            </Box>
          </Box>
        </Toolbar>
      </AppBar>

      {/* Active tab */}
      <Box sx={{ mt: 2 }}>
        {items.map(t => (
          <Box
            key={t.id}
            role="tabpanel"
            hidden={t.id !== active}
            id={`tabpanel-${t.id}`}
            aria-labelledby={`tab-${t.id}`}
          >
            {t.id === active &&
              (typeof t.content === "function" ? (t.content as () => React.ReactNode)() : t.content)}
          </Box>
        ))}
      </Box>
    </Box>
  );
}
