# PlayWrightSauceDemo

C# UI test automation for [saucedemo.com](https://www.saucedemo.com), built with **.NET 10**, **NUnit 4**, and **Microsoft.Playwright.NUnit** (`PageTest` base class). The repo follows a requirement → manual test cases → automation → self-heal pipeline, with dedicated Claude Code agents/skills/commands for each stage (see [CLAUDE.md](CLAUDE.md)).

## Prerequisites

- [.NET SDK 10.x](https://dotnet.microsoft.com/download)
- Playwright browsers (installed via a one-time `playwright.ps1 install` command, see below)

## Setup

1. **Restore & build**

   ```bash
   cd SauceAppTests
   dotnet restore
   dotnet build
   ```

2. **Install Playwright browsers** (one-time, or after a Playwright version bump)

   ```bash
   pwsh bin/Debug/net10.0/playwright.ps1 install --with-deps
   ```

3. **Configure credentials.** No credential values are ever hardcoded in this repo. Copy the example env file and fill in real values:

   ```bash
   cp .env.example .env
   ```

   `.env` is git-ignored and loaded automatically at test run time by [SauceAppTests/TestSettings.cs](SauceAppTests/TestSettings.cs) via `DotNetEnv` (it walks up from the working directory, so `.env` can live at the repo root or in `SauceAppTests/`). Required variables:

   | Variable | Purpose |
   |---|---|
   | `SAUCE_STANDARD_USERNAME` | SauceDemo's published "successful login" demo account |
   | `SAUCE_LOCKED_OUT_USERNAME` | SauceDemo's published locked-out demo account |
   | `SAUCE_PASSWORD` | Password shared by SauceDemo's demo accounts |

   In CI (Azure Pipelines), these are supplied as secret pipeline variables instead of a `.env` file — see [azure-pipelines.yml](azure-pipelines.yml).

## Running tests

All commands run from `SauceAppTests/` (or pass `--project SauceAppTests`).

Both `.runsettings` files pin `NumberOfTestWorkers` to `1`: fixtures are `[Parallelizable(ParallelScope.Self)]`, but running them in parallel launches multiple headed browser instances at once and exhausts local resources (confirmed by re-running the same failures serially and getting a clean pass), so the suite runs sequentially by design.

```bash
# Run the full suite (Chromium, headed — see SauceAppTests/.runsettings)
dotnet test

# Run against Firefox instead
dotnet test /settings:firefox.runsettings

# Run headless (overrides the .runsettings default, e.g. for CI)
dotnet test -- Playwright.LaunchOptions.Headless=true

# Run a single fixture
dotnet test --filter "FullyQualifiedName~AuthenticationTests"

# Run a single test
dotnet test --filter "Name=Login_WithStandardUser_RedirectsToInventoryPage"

# Run the test for one manual test case ID (matches its [Description("TC-...")] attribute)
dotnet test --filter "Description~TC-LOGIN-005"
```

Each run generates an interactive HTML report under `SauceAppTests/TestResults/` (`test-report.html` for Chromium, `test-report-firefox.html` for Firefox — configured in the respective `.runsettings` file). On any test failure, [PlaywrightTestBase](SauceAppTests/Common/PlaywrightTestBase.cs)'s `[TearDown]` captures a screenshot and attaches it to the report automatically.

## Project layout

```
PlayWrightSauceDemo/
├── CLAUDE.md                     # Project instructions, pipeline, conventions (read this first)
├── azure-pipelines.yml           # CI: restore, build, install browsers, run tests headless, publish results
├── .env.example                  # Template for local credentials — copy to .env (git-ignored)
│
├── TestCases/                    # Manual QA test cases (Markdown, one file per feature)
│   ├── Login.md                  # TC-LOGIN-*
│   ├── Logout.md                 # TC-LOGOUT-*
│   ├── Checkout.md                # TC-CHECKOUT-*
│   └── NonFunctional.md          # TC-NFR-* (visual, network mocking, API, accessibility)
│
├── qa-assets/
│   └── manual-test-cases.md      # Master index of all TestCases/*.md files
│
├── reports/                      # Living reports updated by the pipeline/agents
│   ├── coverage-report.md        # Manual TC ID ↔ automated test traceability
│   ├── execution-report.md       # Log of test run results
│   ├── flaky-tests.md            # Intermittent-failure tracking
│   └── healing-report.md         # Self-heal session history
│
└── SauceAppTests/                 # The .NET test project
    ├── SauceAppTests.csproj       # net10.0, NUnit 4, Microsoft.Playwright.NUnit 1.52.0, etc.
    ├── TestSettings.cs           # Credential accessors (env vars via DotNetEnv) — no literals anywhere else
    ├── .runsettings              # Default run config: Chromium, headed, HTML report
    ├── firefox.runsettings       # Same, targeting Firefox
    │
    ├── Pages/                    # Page Object Model — one class per app page
    │   ├── LoginPage.cs
    │   ├── InventoryPage.cs
    │   ├── CartPage.cs
    │   └── CheckoutPage.cs
    │
    ├── Tests/                    # Test fixtures, grouped by theme (not 1:1 with TestCases files)
    │   ├── AuthenticationTests.cs   # Login.md + Logout.md (share LoginPage/InventoryPage)
    │   ├── CheckoutTests.cs         # Checkout.md
    │   └── NonFunctionalTests.cs    # NonFunctional.md
    │
    ├── Common/
    │   ├── PlaywrightTestBase.cs    # Shared PageTest base: LoginAsStandardUserAsync() + screenshot-on-failure [TearDown]
    │   ├── ErrorMessages.cs         # Single source of truth for user-facing error/validation copy (Login, Checkout)
    │   ├── LoginCredentialSet.cs    # Enum driving the data-driven login TestCase
    │   ├── CheckoutValidationScenario.cs # Enum driving the data-driven checkout field-validation TestCase
    │   └── PostLogoutNavigation.cs  # Enum driving the data-driven post-logout-access TestCase
    │
    ├── DTOs/
    │   ├── CustomerDTO.cs         # Strongly-typed row from TestData/customers.csv, keyed by "Key"
    │   └── ProductDTO.cs          # Strongly-typed row from TestData/products.csv, keyed by "Key"
    │
    ├── Utilities/
    │   ├── CsvExposureHelper.cs     # Generic CsvHelper wrapper; tests bind TestData/*.csv rows to DTOs via ReadCsvToObject<T>()
    │   ├── VisualBaseline.cs        # Custom screenshot-diff comparison (no built-in equivalent in this Playwright .NET version)
    │   └── AccessibilityAssertions.cs # Helpers around Deque.AxeCore.Playwright for a11y scans
    │
    ├── TestData/
    │   ├── customers.csv
    │   └── products.csv
    │
    ├── Baselines/
    │   └── inventory-page.png       # Visual-regression baseline image (TC-NFR-001)
    │
    └── TestResults/                 # Generated HTML/TRX reports (not source-controlled input)
```

### Claude Code pipeline (`.claude/`)

This repo drives its QA workflow through custom Claude Code agents, skills, and slash commands — see the table in [CLAUDE.md](CLAUDE.md#qa-pipeline) and [.claude/workflows/testing-workflow.md](.claude/workflows/testing-workflow.md) for full detail:

| Stage | Command |
|---|---|
| Framework setup | `/setup-framework` |
| Page Objects | `/create-page-objects` |
| Manual test cases | `/write-test-cases` |
| Automation code | `/automate-tests` |
| Fix failing tests | `/heal-tests` |
| Run tests (execute + auto-heal + reports) | `/run-tests` |
| All of the above, end to end | `/qa-pipeline` |

## Conventions (see CLAUDE.md for full detail)

- **Page Object Model**: no raw locators inside `[Test]` methods for anything reused more than once.
- **Locator priority**: `GetByRole`/`GetByLabel`/`GetByText`/`GetByPlaceholder` first, then `GetByTestId`, then CSS/XPath only as a last resort.
- **Async all the way**: `async Task` + `await`; never `.Result`/`.Wait()`.
- **Auto-waiting only**: Playwright `Locator`s and `Expect()` — never `Task.Delay`/`Thread.Sleep`.
- **No hardcoded credentials anywhere** — always via `TestSettings`.
- Every `[Test]` method references its manual test case ID via `[Description("TC-...")]`.

## Traceability

`reports/coverage-report.md` tracks which manual test cases (`TC-LOGIN-*`, `TC-LOGOUT-*`, `TC-CHECKOUT-*`, `TC-NFR-*`) have corresponding automated tests, and where each lives in `SauceAppTests/Tests/`.
