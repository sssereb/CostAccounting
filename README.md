# CostAccounting

[![CI](https://github.com/sssereb/CostAccounting/actions/workflows/ci.yml/badge.svg)](https://github.com/sssereb/CostAccounting/actions/workflows/ci.yml)

A small portfolio cost-basis accounting app. Buys create lots. Sells consume
lots using FIFO, LIFO or Average cost. Fee rules (fixed per trade, fixed per
share or percent of trade value) apply to buys and sells. Each sale records
gross profit (no fees) and net profit (after buy and sell fees).

## Stack

- Backend: .NET 8 minimal API, layered as Domain / Application /
  Infrastructure.EfCore / Web. EF Core + SQLite with migrations, or an
  in-memory store. Errors are returned as ProblemDetails. Swagger UI at `/swagger`.
- Tests: xUnit + Moq; API tests with WebApplicationFactory on SQLite in-memory.
- Frontend: React 19 + TypeScript + Vite, TanStack Query, MUI. API types are
  generated from the OpenAPI document (openapi-typescript + openapi-fetch).
- CI: GitHub Actions builds and tests both parts on pushes to main and on pull requests.

## Running

Requirements: .NET 8 SDK, Node.js 20.19+.

```
dotnet run --project PortfolioApp/PortfolioApp.DbUpdater   # create or migrate the SQLite database
dotnet run --project PortfolioApp/PortfolioApp.Web         # API under http://localhost:5255/api, plus the built UI
cd PortfolioApp/portfolio-ui && npm install && npm run dev # UI on http://localhost:5173, proxies /api
dotnet test PortfolioApp/PortfolioApp.sln
```

Configuration (environment variables or any .NET configuration source):

- `PORTFOLIO_STORAGE=Sqlite|InMemory` selects the store. Default is `Sqlite`.
- `DB_PATH` overrides the SQLite file. Without it the path comes from
  `database:relativePath` in `PortfolioApp/config/appsettings.shared.json`
  (default `Database/portfolio.db`). Relative paths resolve against `PortfolioApp/`.

After changing an endpoint or DTO, start the backend without building the UI (the UI will not compile
against the old types) and refresh the contract:

```
dotnet run --project PortfolioApp/PortfolioApp.Web -p:SkipFrontendBuild=true
cd PortfolioApp/portfolio-ui && npm run openapi:pull && npm run gen:api
```

A test compares the live OpenAPI document with the committed `openapi.json`,
and CI checks that `src/api/schema.d.ts` was regenerated from it.

## Project layout

```
PortfolioApp/
  PortfolioApp.Domain/                 Entities: Asset, Lot, Trade, Fee
  PortfolioApp.Application/            TradeService, cost-basis strategies, fees, IUnitOfWork, in-memory store
  PortfolioApp.Infrastructure.EfCore/  DbContext, EF Core repositories and unit of work, SQLite migrations
  PortfolioApp.Web/                    Minimal API endpoints, exception handler, Swagger
  PortfolioApp.DbUpdater/              Console tool that applies migrations
  PortfolioApp.Console/                Interactive console client for the same services
  PortfolioApp.Tests/                  Strategy, service, persistence and API tests
  portfolio-ui/                        React frontend, openapi.json snapshot, generated API types
  config/appsettings.shared.json       Shared database path setting
```

## Decisions and trade-offs

- **Layered architecture.** Domain has no dependencies. Application holds the
  rules and the ports (repositories, unit of work). Infrastructure implements
  them with EF Core, and Web only maps HTTP. Cost-basis logic is tested without
  a database, and the store can be swapped. The cost is more projects than an
  app this size strictly needs.
- **Strategy + factory for cost basis.** Each method is a small class; FIFO and
  LIFO only define the lot order and share one template for consuming lots.
  A new method (for example HIFO) needs a class, an enum value and a DI
  registration, then regenerated API types.
- **Fee-inclusive unit cost.** Buy fees are part of the cost basis, so a lot
  stores both the raw price and the unit cost including fees. Gross profit uses
  raw prices; net profit uses the cost basis and subtracts sell fees.
- **Average cost pools the remaining shares.** After an Average sale the lots
  that are left take the averaged cost, so a later FIFO or LIFO sale uses the
  same basis. Merging lots into one was rejected because it loses the purchase
  dates that FIFO and LIFO order by.
- **Optimistic concurrency on lots.** A sale reads lots, computes, then writes
  lots and the trade in one short transaction. Each lot has a version checked
  on update; if another sale got there first, the transaction rolls back and
  the API returns 409. No locks are held while computing, and it works on
  SQLite, which has no row locks. The losing sale is not retried automatically;
  the user sees the 409 and can submit again.
- **Contract from OpenAPI.** The UI types are generated, so an enum or field
  mismatch fails `tsc` instead of failing at runtime.

What I would do differently in production:

- PostgreSQL, with migrations run as a deployment step instead of a tool.
- Authentication and per-user portfolios.
- Structured logging and tracing (OpenTelemetry), plus health checks.
- Dedicated domain exception types. Today every InvalidOperationException
  maps to 409, including ones thrown by libraries.
- Frontend tests: Vitest for hooks and forms, Playwright for buy and sell flows.
- An explicit rounding policy, fractional shares and time-zone-aware dates.

## Known limitations

- The in-memory store has no transactions, and strategies update lots in
  place, so a failed sale there can leave partial changes. It is meant for
  demos; SQLite is the real store.
- The Web app does not apply migrations on startup; run the DbUpdater first.
- Fee rules live in memory, reset to two demo rules on restart, and have no
  editing UI yet.
- Trades stored before the profit definitions changed keep the old meaning of
  gross profit (it included buy fees).
- There are no frontend tests.
