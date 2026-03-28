# Calculator API

Minimal ASP.NET Core API that uses `CachedCalculator` from the `Calculator` project and stores every execution in PostgreSQL.

## Environment variable

- `ConnectionStrings__CalculatorDb`
  - Example: `Host=localhost;Port=5432;Database=calculator;Username=calculator_app;Password=calculator_app`

## Run

```bash
dotnet run --project Calculator.Api/Calculator.Api.csproj --urls http://localhost:5000
```

## Endpoints

- `POST /api/calculations`
  - Body: `{ "operation": "add", "a": 2, "b": 3 }`
- `GET /api/calculations/recent?take=10`

