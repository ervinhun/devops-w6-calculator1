using Calculator.Api.Contracts;

namespace Calculator.Api.Persistence;

public interface ICalculationHistoryRepository
{
    Task InsertAsync(string operation, int a, int? b, string? resultText, bool success, string? errorMessage);
    Task<IReadOnlyList<HistoryItem>> GetRecentAsync(int take);
}

