# Manual Test Cases — Master Index

This file is the **index** of all manual test case files in this repo. Each feature's test cases live in their own file under `TestCases/`. Update this index whenever a new `TestCases/<Feature>.md` file is created.

---

## Coverage Summary

| Feature | File | Total TCs | Automated? | Last Updated |
|---|---|---|---|---|
| Login | `TestCases/Login.md` | — | — | — |
| Inventory / Product List | `TestCases/Inventory.md` | — | — | — |
| Product Detail | `TestCases/ProductDetail.md` | — | — | — |
| Shopping Cart | `TestCases/Cart.md` | — | — | — |
| Checkout | `TestCases/Checkout.md` | — | — | — |

> **How to add a row:** After running `/write-test-cases <feature>`, add a row here with the file path, count of TCs, and today's date.

---

## ID Scheme

```
TC-<FEATURE>-<NUM>
```

| Feature token | Prefix |
|---|---|
| Login | `TC-LOGIN-` |
| Inventory | `TC-INV-` |
| Product Detail | `TC-PROD-` |
| Cart | `TC-CART-` |
| Checkout | `TC-CHKOUT-` |

Numbers are zero-padded to three digits: `TC-LOGIN-001`, `TC-LOGIN-002`, …

---

## Traceability

Every `[Test]` method in `SauceAppTests/Tests/` must carry a `[Description("TC-...")]` attribute referencing the manual test case ID it implements.

To audit coverage:

```powershell
# Find all manual TC IDs
Select-String -Path TestCases\*.md -Pattern "TC-[A-Z]+-\d{3}"

# Find all automated TC IDs
Select-String -Path SauceAppTests\Tests\*.cs -Pattern "TC-[A-Z]+-\d{3}"
```

IDs that appear in `TestCases/` but not in `Tests/` are **not yet automated**.
