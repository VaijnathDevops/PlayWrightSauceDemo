# Test Coverage Report

Tracks the traceability between manual test case IDs and automated test methods. Update whenever new test cases or automation is added.

---

## Coverage Matrix

| TC ID | Manual Test Case | Automated? | Test Method | File |
|---|---|---|---|---|
| TC-LOGIN-001 | — | ❌ | — | — |
| TC-LOGIN-002 | — | ❌ | — | — |
| TC-INV-001 | — | ❌ | — | — |
| TC-CART-001 | — | ❌ | — | — |
| TC-CHKOUT-001 | — | ❌ | — | — |

> Fill in as test cases and automation are created. Use `/write-test-cases` and `/automate-tests` commands to generate them.

---

## Summary

| Metric | Count |
|---|---|
| Total manual TCs | 0 |
| Automated | 0 |
| Not automated | 0 |
| **Automation coverage** | **0%** |

---

## How to Audit Coverage

```powershell
# All manual TC IDs defined
Select-String -Path TestCases\*.md -Pattern "TC-[A-Z]+-\d{3}" | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value | Sort-Object -Unique

# All automated TC IDs referenced
Select-String -Path TestProject1\Tests\*.cs -Pattern "TC-[A-Z]+-\d{3}" | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value | Sort-Object -Unique
```

IDs in the first list but not the second = **automation gap**.

---

## Coverage Goals

| Phase | Target |
|---|---|
| MVP | Core happy-path flows (Login, Inventory, Checkout) |
| Phase 2 | All negative / error cases |
| Phase 3 | Edge cases, boundary values, multi-user scenarios |
