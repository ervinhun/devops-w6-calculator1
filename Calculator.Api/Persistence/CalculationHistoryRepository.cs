using Calculator.Api.Contracts;
using Npgsql;

namespace Calculator.Api.Persistence;

public sealed class CalculationHistoryRepository(string connectionString) : ICalculationHistoryRepository
{
    public async Task InsertAsync(string operation, int a, int? b, string? resultText, bool success, string? errorMessage)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO calculator_operations (operation, operand_a, operand_b, result_text, success, error_message)
            VALUES (@operation, @operand_a, @operand_b, @result_text, @success, @error_message);";

        cmd.Parameters.AddWithValue("operation", operation);
        cmd.Parameters.AddWithValue("operand_a", a);
        cmd.Parameters.AddWithValue("operand_b", (object?)b ?? DBNull.Value);
        cmd.Parameters.AddWithValue("result_text", (object?)resultText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("success", success);
        cmd.Parameters.AddWithValue("error_message", (object?)errorMessage ?? DBNull.Value);

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
            int? operandB = await reader.IsDBNullAsync(3) ? null : reader.GetInt32(3);
            var resultText = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
            var errorMessage = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
            var createdAt = await reader.GetFieldValueAsync<DateTimeOffset>(7);

            items.Add(new HistoryItem(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetInt32(2),
                operandB,
                resultText,
                reader.GetBoolean(5),
                errorMessage,
                createdAt
            ));
        }

        return items;
    }
}


