using Calculator;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__CalculatorDb")
    ?? "Host=localhost;Port=5432;Database=calculator;Username=calculator_app;Password=calculator_app";

builder.Services.AddSingleton<ICalculator, CachedCalculator>();
builder.Services.AddSingleton(new CalculationHistoryRepository(connectionString));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

app.MapPost("/api/calculations", async (CalculationRequest request, ICalculator calculator, CalculationHistoryRepository historyRepository) =>
{
    var operation = request.Operation?.Trim();
    if (string.IsNullOrWhiteSpace(operation))
    {
        return Results.BadRequest(new { error = "Operation is required." });
    }

    var normalizedOperation = NormalizeOperation(operation);
    var cacheCountBefore = calculator is CachedCalculator cachedBefore ? cachedBefore.Cache.Count : 0;

    try
    {
        var result = Execute(calculator, normalizedOperation, request.A, request.B);
        var cacheCountAfter = calculator is CachedCalculator cachedAfter ? cachedAfter.Cache.Count : 0;

        var response = new CalculationResponse(
            normalizedOperation,
            request.A,
            request.B,
            result,
            cacheCountAfter == cacheCountBefore
        );

        await historyRepository.InsertAsync(new HistoryWriteModel(
            normalizedOperation,
            request.A,
            request.B,
            response.Result,
            true,
            null));

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        await historyRepository.InsertAsync(new HistoryWriteModel(
            normalizedOperation,
            request.A,
            request.B,
            null,
            false,
            ex.Message));

        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/calculations/recent", async (int? take, CalculationHistoryRepository historyRepository) =>
{
    var safeTake = Math.Clamp(take ?? 10, 1, 100);
    var items = await historyRepository.GetRecentAsync(safeTake);
    return Results.Ok(items);
});

app.Run();

static string Execute(ICalculator calculator, string operation, int a, int? b)
{
    return operation switch
    {
        "Add" => calculator.Add(a, RequireSecondOperand(b, operation)).ToString(),
        "Subtract" => calculator.Subtract(a, RequireSecondOperand(b, operation)).ToString(),
        "Multiply" => calculator.Multiply(a, RequireSecondOperand(b, operation)).ToString(),
        "Divide" => calculator.Divide(a, RequireSecondOperand(b, operation)).ToString(),
        "Factorial" => calculator.Factorial(a).ToString(),
        "IsPrime" => calculator.IsPrime(a).ToString().ToLowerInvariant(),
        _ => throw new ArgumentException($"Unsupported operation: {operation}")
    };
}

static int RequireSecondOperand(int? value, string operation)
{
    if (!value.HasValue)
    {
        throw new ArgumentException($"Operation '{operation}' requires operand b.");
    }

    return value.Value;
}

static string NormalizeOperation(string operation)
{
    return operation.ToLowerInvariant() switch
    {
        "add" => "Add",
        "subtract" => "Subtract",
        "multiply" => "Multiply",
        "divide" => "Divide",
        "factorial" => "Factorial",
        "isprime" => "IsPrime",
        _ => throw new ArgumentException($"Unsupported operation: {operation}")
    };
}

record CalculationRequest(string Operation, int A, int? B);

record CalculationResponse(
    string Operation,
    int A,
    int? B,
    string Result,
    bool FromCache
);

record HistoryWriteModel(
    string Operation,
    int A,
    int? B,
    string? ResultText,
    bool Success,
    string? ErrorMessage
);

record HistoryItem(
    long Id,
    string Operation,
    int OperandA,
    int? OperandB,
    string? ResultText,
    bool Success,
    string? ErrorMessage,
    DateTimeOffset CreatedAt
);

sealed class CalculationHistoryRepository(string connectionString)
{
    public async Task InsertAsync(HistoryWriteModel item)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO calculator_operations (operation, operand_a, operand_b, result_text, success, error_message)
            VALUES (@operation, @operand_a, @operand_b, @result_text, @success, @error_message);";

        cmd.Parameters.AddWithValue("operation", item.Operation);
        cmd.Parameters.AddWithValue("operand_a", item.A);
        cmd.Parameters.AddWithValue("operand_b", (object?)item.B ?? DBNull.Value);
        cmd.Parameters.AddWithValue("result_text", (object?)item.ResultText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("success", item.Success);
        cmd.Parameters.AddWithValue("error_message", (object?)item.ErrorMessage ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<HistoryItem>> GetRecentAsync(int take)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, operation, operand_a, operand_b, result_text, success, error_message, created_at
            FROM calculator_operations
            ORDER BY created_at DESC
            LIMIT @take;";
        cmd.Parameters.AddWithValue("take", take);

        var items = new List<HistoryItem>(take);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new HistoryItem(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetBoolean(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.GetFieldValue<DateTimeOffset>(7)
            ));
        }

        return items;
    }
}

