export type Operation = "add" | "subtract" | "multiply" | "divide" | "factorial" | "isPrime";

export type CalculationRequest = {
  operation: Operation;
  a: number;
  b?: number;
};

export type CalculationResponse = {
  operation: string;
  a: number;
  b?: number;
  result: string;
  fromCache: boolean;
};

export type HistoryItem = {
  id: number;
  operation: string;
  operandA: number;
  operandB?: number | null;
  resultText?: string | null;
  success: boolean;
  errorMessage?: string | null;
  createdAt: string;
};

