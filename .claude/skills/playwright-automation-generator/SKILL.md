---
name: playwright-automation-generator
description: Use when converting a manual test case file (TestCases/*.md) into C# Playwright/NUnit automation code in SauceAppTests, following this repo's Page Object Model conventions.
---

# Playwright Automation Generator

Converts a manual test case Markdown file into compiling, running C# Playwright (`Microsoft.Playwright.NUnit`) test code.

## Procedure

1. **Read the test case file fully.** Build a mental (or literal, in your scratch notes) map of ID → steps → expected result before writing any code.

2. **Identify required Page Objects.** For each distinct screen/component touched by the steps (e.g. LoginPage, InventoryPage, CartPage, CheckoutPage):
   - Glob `SauceAppTests/Pages/*.cs` — if one already exists for that screen, extend it; don't create a duplicate.
   - If it doesn't exist, create `SauceAppTests/Pages/<Screen>Page.cs`.

3. **Resolve real selectors — never invent one.** For each element a step interacts with:
   - Check if it's already used in an existing Page Object/test in the repo.
   - Otherwise inspect the real app: navigate to it (e.g. via a throwaway `dotnet run`/small script, or `npx playwright codegen <url>` if the tool is available in this environment) and read the actual DOM/accessibility tree.
   - If neither is possible in this session, stop and ask the user for the selector/markup rather than guessing a `data-test` attribute or class name.
   - **Prefer, in order: `GetByRole`, `GetByLabel`, `GetByText`, `GetByPlaceholder`, `GetByTestId`, then CSS/`#id` as a last resort.** Don't reach for an `#id`/CSS selector just because it's the first thing that works or is simpler to write — actually check whether a role, label, visible text, or placeholder identifies the element first, since that's what this repo's convention (and the user) explicitly wants. An element having an `id` is not a reason to skip the higher tiers.
   - `GetByTestId` matches Playwright's **default `data-testid` attribute**, not SauceDemo's `data-test` attribute — it silently matches nothing against `data-test` unless `Selectors.SetTestIdAttribute("data-test")` has been configured somewhere in the test setup. Check whether that's already wired up before using `GetByTestId`; if it isn't, either wire it up (verify where it needs to live for it to take effect — e.g. a shared one-time setup — by actually building and running a test that depends on it) or use a raw CSS attribute selector (`Page.Locator("[data-test='...']")`) instead and say so, rather than writing a `GetByTestId` call you haven't confirmed actually matches.

4. **Write the Page Object** — constructor takes `IPage page`, exposes `Locator` properties/methods and async action methods (e.g. `Task LoginAsync(string user, string pass)`), no assertions inside Page Objects (assertions belong in the test). Credential parameters are always passed in by the caller — a Page Object never has a literal username/password/token baked in.

5. **Choose a test file — group by theme, don't default to one file per manual test case file.** Before creating `SauceAppTests/Tests/<Feature>Tests.cs`, check whether an existing fixture already covers the same or a closely related theme (same Page Objects, same area of the app) and extend it instead of creating a new file. E.g. a "Logout" manual test case file belongs in the same fixture as "Login" (both are Authentication, both use `LoginPage`) — not a separate `LogoutTests.cs`. Keep genuinely distinct concerns (a different area of the app, or a different *kind* of testing — e.g. functional flows vs. cross-cutting quality checks like visual/accessibility/network) in their own files rather than merging everything into one, but don't multiply files beyond what the grouping actually justifies. A C# file can and should hold more than one `[TestFixture]` class when that keeps related fixtures together without merging unrelated ones into a single bloated class.

6. **Write the test fixture(s):**
   - `[TestFixture]` class inheriting `PageTest`, `[Parallelizable(ParallelScope.Self)]` matching the existing scaffold style.
   - One `[Test]` method per manual test case (or one `[TestCase]`-parameterized method for a family of similar cases) with `[Description("TC-...")]` naming the case it implements.
   - Steps become sequential `await` calls on Page Object methods; the manual case's Expected Result becomes one or more `await Expect(...)` assertions.
   - Method names: `Scenario_Condition_ExpectedResult` in PascalCase, e.g. `Login_WithLockedOutUser_ShowsLockedOutError`.
   - **Credentials come from `TestSettings`** (`SauceAppTests/TestSettings.cs`), e.g. `TestSettings.StandardUsername`, never a literal string in the test body. For a "wrong password" case, use a clearly-fake value (e.g. a local `const string WrongPassword = "not-the-real-password"`) rather than any value that happens to look like a real account credential. See CLAUDE.md's "Credentials & Sensitive Data" section — this applies even to SauceDemo's own public demo accounts.
   - **Use `[SetUp]` to remove duplication, not decoratively.** If most/all tests in a fixture repeat the same `var loginPage = new LoginPage(Page); var inventoryPage = new InventoryPage(Page);`-style instantiation, hoist it into `[SetUp]` as private fields instead of repeating it in every test method. If *every* test in the fixture also shares one genuinely uniform precondition step (e.g. all Authentication tests start by navigating to the login page), hoist that too. Only hoist what's true for every test in the fixture — don't force a shared step (e.g. logging in) into `[SetUp]` if even one test needs a different starting state (e.g. an accessibility scan of the logged-out login page alongside one of the logged-in inventory page); in that case leave that step in the individual test bodies that need it.
   - **Use `[TearDown]` for real value, not just because it exists.** A good default: capture a screenshot when the test failed (`TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed`), written under `TestContext.CurrentContext.WorkDirectory` or similar, to aid debugging failures without cluttering every test body. This is additive to, not a replacement for, `PageTest`'s own lifecycle — never manually create or dispose `IBrowser`/`IBrowserContext`/`IPage` yourself; that's still fully owned by the base class per CLAUDE.md.

7. **Build.** Run:
   ```
   dotnet build
   ```
   in `SauceAppTests/`. Fix any compile errors before proceeding — do not hand back code that doesn't compile.

8. **Run it.**
   ```
   dotnet test --filter "FullyQualifiedName~<FeatureTests>"
   ```
   Report actual pass/fail counts. If anything fails, either fix it now (same session) or explicitly hand off to the self-heal procedure (`test-self-heal` skill) — don't report the task done with red tests. If you extended an existing fixture/file, also rerun that whole file's tests (not just the new ones) to confirm nothing regressed.

9. **Traceability check.** Confirm every ID in the manual test case file has a corresponding `[Test]`/`[TestCase]` in the generated code (or note explicitly which ones were deferred and why).

## Anti-hallucination

- Only use Playwright members you can verify exist for `Microsoft.Playwright.NUnit` 1.52.0 and NUnit 4.3.2 (this repo's installed versions) — check `SauceAppTests.csproj` if unsure which versions are in play.
- Never assert on exact text you haven't confirmed — if the manual test case marked copy `[TBD]`, either resolve it by inspecting the real app first, or use a looser but still meaningful assertion (e.g. `ToContainTextAsync` on a regex) and flag it in your summary.
