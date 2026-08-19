# Test Execution Report

Generated after each `dotnet test` run. Update this file (or append a new run block) after every significant test execution.

---

## Run Log

<!-- Add a new block below for each run. Most recent first. -->

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
