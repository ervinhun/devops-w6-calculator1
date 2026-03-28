# Flyway Migrations

This folder holds versioned SQL migrations for calculator history storage.

## Target database

- PostgreSQL 15+

## Tables

- `calculator_operations`: immutable log of calculations and errors.

The database is intentionally history-only. Caching stays in application memory.

## Local run example

```bash
flyway -url=jdbc:postgresql://localhost:5432/calculator -user=calculator_app -password=calculator_app -locations=filesystem:flyway/migrations migrate
```

## Required GitHub secrets for CI

- `FLYWAY_URL`
- `FLYWAY_USER`
- `FLYWAY_PASSWORD`

