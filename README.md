# Company Coding Exercise

## Prerequisites

- .NET SDK 10
- Node.js (latest LTS) and npm

## Project Structure

- Backend: `src/Company.Api` (with `Company.Application`, `Company.Domain`, `Company.Infrastructure`)
- Frontend: `frontend/company-ui`
- Tests: `tests/Company.UnitTests`, `tests/Company.IntegrationTests`

## Run the Backend API

From the repository root:

```bash
dotnet run --project src/Company.Api
```

The API runs on:

- `http://localhost:5000`
- `https://localhost:7000`

## Run the Angular UI

From `frontend/company-ui`:

```bash
npm install
npm start
```

The UI runs on:

- `http://localhost:4200`

The frontend proxies `/api` calls to `http://localhost:5000`.

## Run Tests

From the repository root:

```bash
dotnet test Verus.slnx
```
