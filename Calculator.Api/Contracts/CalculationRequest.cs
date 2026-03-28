namespace Calculator.Api.Contracts;

public record CalculationRequest(string? Operation, int A, int? B);

