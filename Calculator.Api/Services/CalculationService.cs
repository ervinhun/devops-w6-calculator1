using Calculator;
using Calculator.Api.Contracts;
using Calculator.Api.Persistence;

namespace Calculator.Api.Services;

public sealed class CalculationService(ICalculator calculator, ICalculationHistoryRepository historyRepository)
    : ICalculationService
{
    public const int MaxFactorialInput = 12;

    public async Task<CalculationResponse> ExecuteAsync(CalculationRequest request)
    {
        var operation = request.Operation?.Trim();
        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new CalculationValidationException("Operation is required.");
        }

        string normalizedOperation;
        try
        {
            normalizedOperation = NormalizeOperation(operation);
        }
        catch (ArgumentException ex)
        {
            throw new CalculationValidationException(ex.Message);
        }

        var cacheCountBefore = calculator is CachedCalculator cachedBefore ? cachedBefore.Cache.Count : 0;

        try
        {
            var result = Execute(normalizedOperation, request.A, request.B);
            var cacheCountAfter = calculator is CachedCalculator cachedAfter ? cachedAfter.Cache.Count : 0;

            var historyPersisted = await TryInsertHistoryAsync(
                normalizedOperation,
                request.A,
                request.B,
                result,
                true,
                null);

            return new CalculationResponse(
                normalizedOperation,
                request.A,
                request.B,
                result,
                cacheCountAfter == cacheCountBefore,
                historyPersisted);
        }
        catch (Exception ex)
        {
            await TryInsertHistoryAsync(
                normalizedOperation,
                request.A,
                request.B,
                null,
                false,
                ex.Message);

            throw new CalculationValidationException(ex.Message);
        }
    }

    public async Task<IReadOnlyList<HistoryItem>> GetRecentAsync(int? take)
    {
        var safeTake = Math.Clamp(take ?? 10, 1, 100);

        try
        {
            return await historyRepository.GetRecentAsync(safeTake);
        }
        catch
        {
            throw new HistoryUnavailableException("Could not load calculation history.");
        }
    }

    private async Task<bool> TryInsertHistoryAsync(
        string operation,
        int a,
        int? b,
        string? resultText,
        bool success,
        string? errorMessage)
    {
        try
        {
            await historyRepository.InsertAsync(operation, a, b, resultText, success, errorMessage);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string Execute(string operation, int a, int? b)
    {
        return operation switch
        {
            "Add" => calculator.Add(a, RequireSecondOperand(b, operation)).ToString(),
            "Subtract" => calculator.Subtract(a, RequireSecondOperand(b, operation)).ToString(),
            "Multiply" => calculator.Multiply(a, RequireSecondOperand(b, operation)).ToString(),
            "Divide" => calculator.Divide(a, RequireSecondOperand(b, operation)).ToString(),
            "Factorial" => calculator.Factorial(ValidateFactorialInput(a)).ToString(),
            "IsPrime" => calculator.IsPrime(a).ToString().ToLowerInvariant(),
            _ => throw new ArgumentException($"Unsupported operation: {operation}")
        };
    }

    private static int RequireSecondOperand(int? value, string operation)
    {
        if (!value.HasValue)
        {
            throw new ArgumentException($"Operation '{operation}' requires operand b.");
        }

        return value.Value;
    }

    private static int ValidateFactorialInput(int n)
    {
        if (n < 0)
        {
            throw new ArgumentException("Factorial is not defined for negative numbers");
        }

        if (n > MaxFactorialInput)
        {
            throw new ArgumentException(
                $"Factorial supports values up to {MaxFactorialInput} to protect service stability.");
        }

        return n;
    }

    private static string NormalizeOperation(string operation)
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
}

