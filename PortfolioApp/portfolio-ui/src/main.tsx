import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import "./index.css";
import "ag-grid-community/styles/ag-grid.css";        // базовые правила
import "ag-grid-community/styles/ag-theme-alpine.css"; // любая тема: alpine, quartz, material, balham, etc.
import "react-datepicker/dist/react-datepicker.css";


const qc = new QueryClient();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <QueryClientProvider client={qc}>
      <App />
    </QueryClientProvider>
  </React.StrictMode>
);
