# Calculator Frontend and Database Plan

## Frontend plan (`/client`, TypeScript)

- Stack: Vite + TypeScript + plain HTML/CSS for a small learning-friendly UI.
- Features:
  - Choose operation (`Add`, `Subtract`, `Multiply`, `Divide`, `Factorial`, `IsPrime`).
  - Enter operands and calculate.
  - Show result and basic error messages (for example divide-by-zero).
  - Keep a short recent history panel.
- Future backend integration:
  - Replace in-browser calculation with API calls to a calculator backend.
  - POST to `/api/calculations` with operation + operands.
  - GET `/api/calculations/recent` for persisted history.

## Database plan (PostgreSQL)

- `calculator_operations`
  - Immutable audit/history log for each request.
  - Stores operation name, operands, success/error, result, and timestamp.
- `v_recent_calculator_operations`
  - Read model for UI/reporting with latest operation records.

The database scope is history only; any cache behavior stays in application memory.

## Flyway migration strategy

- `V1`: create base history table.
- `V2`: add constraints and indexes for history filtering and timeline queries.
- `V3`: add convenience view for recent records.
- `V4`: remove legacy cache table if an earlier migration run created it.

## Deployment plan

- A dedicated GitHub Actions workflow runs Flyway `validate` then `migrate`.
- It is triggered automatically when migration files change and can also run manually.
- It exports `flyway-report/info.json` as an artifact for traceability.

