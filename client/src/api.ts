import type { CalculationRequest, CalculationResponse, HistoryItem } from "./types";

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

export async function executeCalculation(payload: CalculationRequest): Promise<CalculationResponse> {
  const response = await fetch(`${baseUrl}/api/calculations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as { error?: string } | null;
    throw new Error(error?.error ?? "Calculation request failed");
  }

  return (await response.json()) as CalculationResponse;
}

export async function fetchRecentHistory(take = 10): Promise<HistoryItem[]> {
  const response = await fetch(`${baseUrl}/api/calculations/recent?take=${take}`);

  if (!response.ok) {
    throw new Error("Failed to load history");
  }

  return (await response.json()) as HistoryItem[];
}

