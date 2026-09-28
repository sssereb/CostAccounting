# CostAccounting

[![CI](https://github.com/sssereb/CostAccounting/actions/workflows/ci.yml/badge.svg)](https://github.com/sssereb/CostAccounting/actions/workflows/ci.yml)

A small portfolio cost-basis accounting app. Buys create lots. Sells consume
lots using FIFO, LIFO or Average cost. Configurable fee rules (fixed per trade,
fixed per share or percent of trade value) are applied to buys and sells.
Each sale records realized profit gross and net of fees.

## Stack

- Backend: .NET 8 minimal API, layered as Domain / Application /
  Infrastructure.EfCore / Web. EF Core + SQLite with migrations, or an
  in-memory store. Swagger UI is enabled.
- Tests: xUnit + Moq.
- Frontend: React 19 + TypeScript + Vite, TanStack Query, MUI (incl. X Data Grid).

## Running

Requirements: .NET 8 SDK, Node.js 20.19+.

Create or update the SQLite database (applies EF Core migrations):

```
dotnet run --project PortfolioApp/PortfolioApp.DbUpdater
```

Backend, listens on http://localhost:5255 (Swagger at `/swagger`):

```
dotnet run --project PortfolioApp/PortfolioApp.Web
```

Environment variables:

- `PORTFOLIO_STORAGE=Sqlite|InMemory` selects the store. Default is `Sqlite`.
- The SQLite file location comes from `database:relativePath` in
  `PortfolioApp/config/appsettings.shared.json` (default `Database/portfolio.db`,
  relative to `PortfolioApp/`). `DB_PATH` is read by the Web project but not
  applied yet, see Known limitations.

Frontend, dev server on http://localhost:5173, proxies `/api` to port 5255:

```
cd PortfolioApp/portfolio-ui && npm install && npm run dev
```

Tests:

```
dotnet test PortfolioApp/PortfolioApp.sln
```

## Project layout

```
PortfolioApp/
  PortfolioApp.Domain/                 Entities: Asset, Lot, Trade, Fee
  PortfolioApp.Application/            TradeService, cost-basis strategies, fee rules, in-memory repositories
  PortfolioApp.Infrastructure.EfCore/  DbContext, EF Core repositories, SQLite migrations
  PortfolioApp.Web/                    Minimal API endpoints, Swagger, CORS
  PortfolioApp.DbUpdater/              Console tool that applies migrations
  PortfolioApp.Console/                Interactive console client for the same services
  PortfolioApp.Tests/                  xUnit tests for strategies, fees and TradeService
  portfolio-ui/                        React frontend
  config/appsettings.shared.json       Shared database path setting
```

## Known limitations

- `TradeService.SellAsync` is not transactional: updated lots and the sell
  trade are saved separately, so a failure in between leaves them inconsistent.
- Error handling is demo-level.
- There are no frontend tests.
- The database path is configured only through `appsettings.shared.json`;
  the `DB_PATH` environment variable is ignored.
