using Calculator.Api.Contracts;

namespace Calculator.Api.Services;

public interface ICalculationService
{
    Task<CalculationResponse> ExecuteAsync(CalculationRequest request);
    Task<IReadOnlyList<HistoryItem>> GetRecentAsync(int? take);
}

