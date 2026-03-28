namespace Calculator.Api.Contracts;

public record CalculationResponse(
    string Operation,
    int A,
    int? B,
    string Result,
    bool FromCache,
    bool HistoryPersisted
);

