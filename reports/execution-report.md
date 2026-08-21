# Test Execution Report

Generated after each `dotnet test` run. Update this file (or append a new run block) after every significant test execution.

---

## Run Log

<!-- Add a new block below for each run. Most recent first. -->

### Run — 2026-08-21 (confirmation re-run, post-healing)

| Field | Value |
|---|---|
| Date / Time | 2026-08-21 17:59 |
| Branch | `main` |
| Trigger | Manual — confirmation re-run after healing pass (healer made no code changes; both prior failures diagnosed as genuine app bugs and left red per policy) |
| Command | `dotnet test SauceAppTests/SauceAppTests.csproj` (repo root has no top-level .sln/.csproj, so the project file was targeted explicitly; full suite, no `--filter`) |
| Duration | 1 m 3 s |

#### Results

| Status | Count |
|---|---|
| ✅ Passed | 25 |
| ❌ Failed | 2 |
| ⚠️ Skipped | 0 |
| **Total** | **27** |

#### Failed Tests

| Test | Error Summary | Action Taken |
|---|---|---|
| Checkout_WithEmptyCart_DoesNotProceedToCustomerInfoStep (SauceAppTests.CheckoutTests, line 124) | `PageAssertions.ToHaveURLAsync` expected URL to match `.*cart.html` but was `https://www.saucedemo.com/checkout-step-one.html` — app allows checkout to proceed past the cart with an empty cart instead of blocking it | Not healed — confirmed genuine app bug (BUG-CHECKOUT-001, see `TestCases/Checkout.md`); no code change made; left red per Self-Healing Policy |
| InventoryPage_AccessibilityScan_HasNoCriticalOrSeriousViolations (SauceAppTests.NonFunctionalTests, line 163) | axe-core `select-name` (critical): inventory page's `<select class="product_sort_container" data-test="product-sort-container">` has no accessible name (no label/aria-label/aria-labelledby/title) | Not healed — confirmed genuine app bug, same root cause as `reports/healing-report.md` session 2026-08-19; no code change made; left red per Self-Healing Policy |

#### Notes

Confirmation re-run after the 2026-08-19/2026-08-21 healing pass. Same 2 tests failed with the identical error signatures as the pre-heal run earlier today — no regressions, no new failures, no flakiness observed. Restore emitted `NU1900` warnings (unable to reach `https://pkgs.dev.azure.com/Agdata/_packaging/AGDATA/nuget/v3/index.json` for vulnerability data) — did not block build or test execution. HTML result file written to `SauceAppTests/TestResults/test-report.html`; failure screenshots saved under `SauceAppTests/bin/Debug/net10.0/` for both failing tests.

---

### Run — 2026-08-21

| Field | Value |
|---|---|
| Date / Time | 2026-08-21 17:56 |
| Branch | `main` |
| Trigger | Manual ("run the full Playwright/NUnit test suite") |
| Command | `dotnet test SauceAppTests/SauceAppTests.csproj` (repo root has no top-level .sln/.csproj, so the project file was targeted explicitly; full suite, no `--filter`) |
| Duration | 1 m 19 s |

#### Results

| Status | Count |
|---|---|
| ✅ Passed | 25 |
| ❌ Failed | 2 |
| ⚠️ Skipped | 0 |
| **Total** | **27** |

#### Failed Tests

| Test | Error Summary | Action Taken |
|---|---|---|
| Checkout_WithEmptyCart_DoesNotProceedToCustomerInfoStep (SauceAppTests.CheckoutTests, line 124) | `PageAssertions.ToHaveURLAsync` expected URL to match `.*cart.html` but was `https://www.saucedemo.com/checkout-step-one.html` — app/test navigated past the cart page instead of staying on it when checkout is attempted with an empty cart | Not healed in this run — flagged for `test-healer` |
| InventoryPage_AccessibilityScan_HasNoCriticalOrSeriousViolations (SauceAppTests.NonFunctionalTests, line 163) | axe-core `select-name` (critical): inventory page's `<select class="product_sort_container" data-test="product-sort-container">` has no accessible name (no label/aria-label/aria-labelledby/title) | Previously diagnosed as a genuine app bug (see `reports/healing-report.md` session 2026-08-19) and left unfixed by design — reproduced again on this run, same root cause, not re-healed |

#### Notes

Restore emitted `NU1900` warnings (unable to reach `https://pkgs.dev.azure.com/Agdata/_packaging/AGDATA/nuget/v3/index.json` for vulnerability data) — did not block build or test execution. HTML result file written to `SauceAppTests/TestResults/test-report.html`; failure screenshots saved under `SauceAppTests/bin/Debug/net10.0/` for both failing tests.

---

### Run — 2026-08-19

| Field | Value |
|---|---|
| Date / Time | 2026-08-19 |
| Branch | `main` |
| Trigger | Manual ("run all tests and heal if fails") |
| Command | `dotnet test -- Playwright.LaunchOptions.Headless=true` |
| Duration | 38 s |

#### Results

| Status | Count |
|---|---|
| ✅ Passed | 20 |
| ❌ Failed | 1 |
| ⚠️ Skipped | 0 |
| **Total** | **21** |

#### Failed Tests

| Test | Error Summary | Action Taken |
|---|---|---|
| InventoryPage_AccessibilityScan_HasNoCriticalOrSeriousViolations (TC-NFR-004) | axe-core `select-name` (critical): inventory page's product-sort `<select>` has no accessible name | Diagnosed as genuine app bug, not healed — see `reports/healing-report.md` session 2026-08-19 |

#### Notes

Suite was 18 `[Test]` methods expanding to 21 executed cases (data-driven `TestCase`s on login-credential-set and post-logout-navigation). Ran headless/Chromium via the default `.runsettings` browser with a headless override.

---

## How to Update This File

After running `dotnet test`, capture the summary output and paste it into a new block above. If failures exist, link to the relevant entry in `reports/healing-report.md`.

```powershell
# Run and capture output
dotnet test SauceAppTests/ | Tee-Object -FilePath reports/last-run.txt
```
