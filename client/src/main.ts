import "./styles.css";
import { executeCalculation, fetchRecentHistory } from "./api";
import type { HistoryItem, Operation } from "./types";

type UnaryOperation = "factorial" | "isPrime";

const operationSelect = document.querySelector<HTMLSelectElement>("#operation");
const inputA = document.querySelector<HTMLInputElement>("#a");
const inputB = document.querySelector<HTMLInputElement>("#b");
const calculateButton = document.querySelector<HTMLButtonElement>("#calculate");
const resultOutput = document.querySelector<HTMLOutputElement>("#result");
const historyList = document.querySelector<HTMLUListElement>("#history");

if (!operationSelect || !inputA || !inputB || !calculateButton || !resultOutput || !historyList) {
  throw new Error("Calculator UI failed to initialize.");
}

const unaryOperations: ReadonlySet<Operation> = new Set(["factorial", "isPrime"]);

function updateOperandVisibility(operation: Operation): void {
  const showSecondOperand = !unaryOperations.has(operation);
  inputB.disabled = !showSecondOperand;
}

function renderHistory(items: HistoryItem[]): void {
  historyList.innerHTML = "";

  for (const item of items) {
    const li = document.createElement("li");
    const symbol = symbolForApiOperation(item.operation);
    const expression = item.operandB === null || item.operandB === undefined
      ? `${item.operation}(${item.operandA})`
      : `${item.operandA} ${symbol} ${item.operandB}`;

    li.textContent = item.success
      ? `${expression} = ${item.resultText ?? ""}`
      : `${expression} -> error: ${item.errorMessage ?? "unknown"}`;

    historyList.appendChild(li);
  }
}

async function refreshHistory(): Promise<void> {
  try {
    const items = await fetchRecentHistory(8);
    renderHistory(items);
  } catch {
    historyList.innerHTML = "";
    const li = document.createElement("li");
    li.textContent = "Unable to load history from server.";
    historyList.appendChild(li);
  }
}

operationSelect.addEventListener("change", () => {
  updateOperandVisibility(operationSelect.value as Operation);
});

calculateButton.addEventListener("click", async () => {
  const operation = operationSelect.value as Operation;
  const a = Number(inputA.value);
  const b = Number(inputB.value);

  if (Number.isNaN(a) || (!unaryOperations.has(operation) && Number.isNaN(b))) {
    resultOutput.textContent = "Please enter valid numbers.";
    return;
  }

  try {
    const response = await executeCalculation({
      operation,
      a,
      b: unaryOperations.has(operation) ? undefined : b
    });

    const cacheSuffix = response.fromCache ? " (cache hit)" : "";
    resultOutput.textContent = `${response.result}${cacheSuffix}`;
    await refreshHistory();
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unknown error";
    resultOutput.textContent = message;
    await refreshHistory();
  }
});

function symbolForApiOperation(operation: string): string {
  switch (operation.toLowerCase()) {
    case "add":
      return "+";
    case "subtract":
      return "-";
    case "multiply":
      return "*";
    case "divide":
      return "/";
    default:
      return operation;
  }
}

updateOperandVisibility(operationSelect.value as Operation);
void refreshHistory();

