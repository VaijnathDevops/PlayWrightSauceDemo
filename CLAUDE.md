# PlayWrightSauceDemo — Project Instructions

C# UI test automation project. Stack: .NET 10, NUnit 4, `Microsoft.Playwright.NUnit` 1.52.0 (`PageTest` base class). Target app under test is presumed to be [saucedemo.com](https://www.saucedemo.com) unless a requirement says otherwise — confirm rather than assume when it matters.

## QA Pipeline

This repo works from **requirement → manual test cases → automation → self-heal**. Each stage has a dedicated agent, a skill with the detailed procedure/templates, and a slash command entry point:

| Stage | Command | Agent | Skill |
|---|---|---|---|
| Framework setup | `/setup-framework` | `framework-setup-engineer` | `framework-setup` |
| Page Objects | `/create-page-objects` | `pom-engineer` | `page-object-model-generator` |
| Manual test cases | `/write-test-cases` | `qa-test-designer` | `manual-test-case-writer` |
| Automation code | `/automate-tests` | `playwright-automation-engineer` | `playwright-automation-generator` |
| Fix failing tests | `/heal-tests` | `test-healer` | `test-self-heal` |
| Run tests (execute + auto-heal + reports) | `/run-tests` | `test-runner` (chains into `test-healer`) | `test-execution` |
| All five, end to end | `/qa-pipeline` | (chains the above) | see `.claude/workflows/testing-workflow.md` |

Full pipeline detail: [.claude/workflows/testing-workflow.md](.claude/workflows/testing-workflow.md).

## Repository Conventions

- Manual test cases live in `TestCases/`, one Markdown file per feature: `TestCases/<Feature>.md`.
- Page Objects live in `SauceAppTests/Pages/` (e.g. `LoginPage.cs`, `InventoryPage.cs`).
- Test fixtures live in `SauceAppTests/Tests/`, grouped by theme rather than one file per manual test case file — e.g. `AuthenticationTests.cs` covers both `TestCases/Login.md` and `TestCases/Logout.md` since they share the same Page Objects and concern. Before adding a new file, check whether an existing fixture already covers a closely related theme and extend it instead.
- Every automated `[Test]` method must reference the manual test case ID it implements (in a `[Description("TC-...")]` attribute or a leading comment), so manual and automated coverage stay traceable to each other.

## Code Guidelines (C# / Playwright / NUnit)

- **Page Object Model**: no raw locators inside `[Test]` methods for anything reused more than once — put it on a Page Object.
- **Async all the way**: `async Task`, always `await`. Never `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on Playwright calls — that deadlocks in this SDK.
- **Auto-waiting only**: use Playwright `Locator`s and `Expect()` (`LocatorAssertions` / `PageAssertions`) for waiting and assertions. Never `Task.Delay`/`Thread.Sleep` to "wait for" UI state — that's a smell the automation is fighting, not testing.
- **Locator priority**: user-visible locators first — `GetByRole`, `GetByLabel`, `GetByText`, `GetByPlaceholder` — then `GetByTestId`, then raw CSS/XPath only when nothing above applies. Don't default to an `#id`/CSS selector out of convenience just because it's simpler to write — actually try the higher tiers against the live app first. Note: `GetByTestId` matches Playwright's default `data-testid` attribute, not SauceDemo's `data-test` attribute — it only works once `Selectors.SetTestIdAttribute("data-test")` has been configured (verify where/whether that's wired up before assuming `GetByTestId` will match anything).
- **Naming**: test methods describe scenario and expectation, e.g. `Login_WithInvalidPassword_ShowsErrorMessage`. Use the block-scoped `namespace SauceAppTests { }` style.
- **No hardcoded secrets**: read credentials via `TestSettings` (`SauceAppTests/TestSettings.cs`), never inline. Locally it loads `.env` (copy `.env.example` to `.env`, git-ignored) via `DotNetEnv`; in CI, `azure-pipelines.yml` maps Azure Pipelines secret variables to the same env var names so no code changes are needed between local and CI runs.
- **Data-driven cases**: use `[TestCase]`/`[Values]` for the same flow with different inputs instead of copy-pasted test methods.
- **Use `[SetUp]`/`[TearDown]` to cut duplication, don't skip them out of habit.** `[SetUp]` should instantiate the Page Object(s) shared across a fixture's tests (as private fields) instead of repeating `new LoginPage(Page)`-style lines in every test, and should hoist any single precondition step that's genuinely true for *every* test in that fixture (e.g. all Authentication tests start by navigating to the login page) — but don't force a step into `[SetUp]` if even one test needs a different starting state. `[TearDown]` is worth using for real value, e.g. capturing a screenshot on failure to aid debugging. None of this replaces or duplicates what `PageTest` itself already manages — never manually create/dispose `IBrowser`/`IBrowserContext`/`IPage` yourself.

## Credentials & Sensitive Data

No stage of the pipeline — manual test cases, Page Objects, test fixtures, or helper/constants classes — may contain a literal credential value (username, password, token, API key), even SauceDemo's own published demo values. This is a blanket rule regardless of how "public" a given value seems, so the pattern holds if the app under test ever changes to real, non-public accounts.

- **Source of truth**: `SauceAppTests/TestSettings.cs` — exposes credentials as properties (`StandardUsername`, `LockedOutUsername`, `Password`) read from environment variables. Locally these come from `.env` (copy `.env.example`, git-ignored) via `DotNetEnv`; in CI, `azure-pipelines.yml` maps Azure Pipelines secret variables to the same names.
- **Manual test cases** (`TestCases/*.md`): reference credentials by role and point to their source — e.g. "Standard-user credentials (`TestSettings.StandardUsername` / `.Password`, see `.env.example`)" — never write the literal string.
- **Automation code**: Page Objects accept credentials as method parameters (e.g. `LoginAsync(string username, string password)`) and never embed a value themselves; test fixtures pass `TestSettings.StandardUsername` etc., never a literal. Non-sensitive constants (base URL, page paths, product names) are fine as literals/constants — this rule is about credentials specifically.
- **Self-healing**: a test failing because a credential/env var is missing or wrong is a config problem — fix `.env`/the pipeline secret variable, never hardcode the value into source to make the failure go away.

## Anti-Hallucination Rules

These exist because the biggest failure mode in generated Playwright/NUnit code is *confidently inventing things that don't exist*.

1. **Never invent a selector.** If the exact selector for an element isn't already known from the manual test case, existing Page Objects, or a source you've actually inspected, say so and either inspect the real page (navigate with Playwright/Bash, use `codegen`, or ask the user for the markup/selector) — don't guess a plausible-looking `data-test` attribute.
2. **Never invent an API member.** `Microsoft.Playwright` and `NUnit` have specific, versioned surfaces (Playwright 1.52.0, NUnit 4.3.2 here). Before using an unfamiliar method/property, verify it exists — grep the installed package sources/IntelliSense, or use only well-established members (`GotoAsync`, `ClickAsync`, `FillAsync`, `Locator`, `Expect`, `ToHaveTextAsync`, etc.). If unsure, say "unverified" rather than presenting a guess as fact.
3. **Never claim a test passes without running it.** "Should work" is not "passed". Run `dotnet test` (or a filtered subset) and report actual output.
4. **Never fabricate file paths or class names** that weren't just created or confirmed to exist — check with Glob/Grep/Read first.
5. **State assumptions explicitly.** When a requirement is ambiguous, write the assumption into the manual test case file (or ask) instead of silently picking one interpretation.

## Self-Healing Policy

When a test fails, the fix must address the actual cause, not the symptom:

- Diagnose first: selector drift, timing/race condition, assertion no longer matches real (correct) app behavior, or a genuine app bug.
- Apply the smallest correct fix, rebuild, rerun.
- **Never** "fix" a test by deleting/weakening its core assertion, slapping on a `Thread.Sleep`, or adding `[Ignore]`/`[Explicit]` to make it stop running — unless the user explicitly approves that as the resolution.
- If the fix changes what the test actually verifies, update the corresponding manual test case in `TestCases/` to match, and say so out loud.
- Cap automatic fix attempts at **5 iterations** per test. If still red after that, stop and report the failure with root-cause analysis instead of continuing to guess.
- If the *application* looks genuinely broken (not the test), stop and report it as a product bug candidate — don't force the test green.

Full procedure: `.claude/skills/test-self-heal/SKILL.md`.
