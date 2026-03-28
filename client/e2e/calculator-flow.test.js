import { Selector } from "testcafe";

const baseUrl = process.env.E2E_BASE_URL ?? "http://127.0.0.1:5173";

fixture`Calculator E2E`.page(baseUrl);

test("user can perform add calculation and see result", async (t) => {
  const operation = Selector("#operation");
  const inputA = Selector("#a");
  const inputB = Selector("#b");
  const calculate = Selector("#calculate");
  const result = Selector("#result");

  await t
    .click(operation)
    .click(operation.find("option").withAttribute("value", "add"))
    .selectText(inputA)
    .pressKey("delete")
    .typeText(inputA, "2")
    .selectText(inputB)
    .pressKey("delete")
    .typeText(inputB, "3")
    .click(calculate)
    .expect(result.innerText)
    .contains("5", "Expected result output to include 5");
});


