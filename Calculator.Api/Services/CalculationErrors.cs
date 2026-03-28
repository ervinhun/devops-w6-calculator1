namespace Calculator.Api.Services;

public sealed class CalculationValidationException(string message) : Exception(message);

public sealed class HistoryUnavailableException(string message) : Exception(message);

