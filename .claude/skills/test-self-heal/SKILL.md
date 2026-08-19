---
name: test-self-heal
description: Use when Playwright/NUnit tests in this repo are failing and need iterative root-cause diagnosis and fixing until green (or a genuine blocker is identified). The self-healing loop for TestProject1.
---

# Test Self-Heal Loop

Iteratively diagnoses and fixes failing tests in `TestProject1`, bounded so it can't spiral or fake a pass.

## Loop

For each failing test, repeat up to **5 iterations**:

1. **Run and capture.**
   ```
   dotnet test --filter "FullyQualifiedName~<TestName>"
   ```
   Read the actual NUnit failure message and Playwright error/stack trace — not a summary of it, the real text.

2. **Classify the failure**, using the error text as evidence:
   - **Selector drift** — `TimeoutException` / element not found → the locator no longer matches the DOM. Re-inspect the real page for the current markup; update the Page Object.
   - **Timing/race condition** — intermittent, or "element not stable/visible" errors despite the locator being correct → replace any manual wait with the appropriate `Expect(...)` auto-waiting assertion or `WaitForAsync` on the right state; do not paper over with `Thread.Sleep`.
   - **Assertion drift** — test runs to completion but the actual value differs from expected, and the actual value is the *correct*, intended app behavior → the test's expectation is stale, not the app. Fix the assertion and update the corresponding manual test case in `TestCases/` to match.
   - **Genuine app bug** — actual value is wrong per the requirement/manual test case → this is not a test defect. Stop, report it as a likely product bug, and do not "fix" the test to accept the bug.
   - **Environment/config** — e.g. wrong base URL, missing browser install (`playwright install`), flaky network, or `TestSettings` throwing because a `SAUCE_*` env var isn't set → fix the environment (`.env` locally, the pipeline secret variable in CI), not the test code.

3. **Apply the smallest fix matching the diagnosis.** One change at a time — don't bundle unrelated edits, so each iteration's effect is verifiable.

4. **Rebuild, rerun, record.** After each attempt, note: iteration #, hypothesis, change made, result. This becomes the report if the loop doesn't converge.

5. **Stop conditions**:
   - Test passes → done, move to the next failing test.
   - 5 iterations without a pass → stop, report the iteration log and best root-cause read; hand the decision to the user.
   - Diagnosis is "genuine app bug" → stop immediately, don't burn iterations trying to force green.

## Hard rules (never do these to "make it pass")

- Don't delete, comment out, or loosen the assertion the test exists to verify.
- Don't add `Thread.Sleep`/fixed delays as a substitute for a correct wait condition.
- Don't add `[Ignore]`, `[Explicit]`, or `[CancelAfter]` tricks to stop a test running.
- Don't reduce a specific assertion (`ToHaveTextAsync("exact")`) to a vacuous one (`IsVisible()`) just to get green — only loosen an assertion if the manual test case itself only ever required that looser check, and say so.
- Don't hardcode a credential value (username, password, token) into source to make a missing/invalid config error go away — fix the `.env` file or pipeline secret variable instead. See CLAUDE.md's "Credentials & Sensitive Data" section.

Any of the above requires explicit user sign-off, not an autonomous decision.

## Final report format

| Test | Iterations | Outcome | Root cause | Change made |
|------|-----------|---------|-------------|-------------|
| Login_WithLockedOutUser_ShowsLockedOutError | 1 | Fixed | Selector drift (`.error-message-container` renamed) | Updated `LoginPage.ErrorBanner` locator |
