namespace Calculator.Api.Contracts;

public record HistoryItem(
    long Id,
    string Operation,
    int OperandA,
    int? OperandB,
    string? ResultText,
    bool Success,
    string? ErrorMessage,
    DateTimeOffset CreatedAt
);

