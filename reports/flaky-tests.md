# Flaky Tests Report

Tracks tests that pass and fail intermittently without a code change. Flaky tests undermine CI confidence and must be triaged promptly.

---

## Flaky Test Registry

| Test | Flake Rate | First Seen | Last Seen | Root Cause | Status | Notes |
|---|---|---|---|---|---|---|
| — | — | — | — | — | — | — |

**Status values:** `Investigating` · `Root cause identified` · `Fixed` · `Quarantined` · `Closed`

---

## Flakiness Definitions

A test is **flaky** when:
- It fails on one run and passes on an immediate re-run **with no code change**.
- It fails only in CI but never locally (or vice versa).
- It fails intermittently under load or timing variation.

---

## Common Root Causes (SauceDemo / Playwright)

| Cause | Symptoms | Fix |
|---|---|---|
| Missing `await` on Playwright action | `NullReferenceException` or stale locator | Add `await`; use `async Task` throughout |
| Race condition — no auto-wait | Passes when slow, fails when fast | Replace `Thread.Sleep` with `Expect(...).ToBeVisibleAsync()` |
| Shared browser state | Passes in isolation, fails in suite | Ensure each test uses a fresh `Page` (PageTest does this by default) |
| `performance_glitch_user` timeout | Timeout on login step only | Increase `NavigationTimeout` for that specific test |
| Selector drift after app deploy | Suddenly fails on CI, fine locally | Update locator in Page Object; verify against live app |

---

## Triage Process

1. **Identify**: Note the test name, failure message, and conditions (local vs CI, isolated vs full suite).
2. **Reproduce**: Run the test 5× in a row to estimate flake rate.
3. **Diagnose**: Check the root cause table above; run with `HEADED=1` to observe visually.
4. **Fix or quarantine**: Apply the smallest correct fix. If the root cause is unclear after investigation, add `[Category("flaky")]` temporarily and open a tracking entry in this file — never silently leave a flaky test untracked.
5. **Verify**: Confirm the fix by running the test 5× — all must pass.
6. **Close**: Update the registry row to `Fixed` and remove any `[Category("flaky")]` tag.

---

## Quarantine Policy

A test may be marked `[Category("flaky")]` and excluded from the default CI run **only** if:

- The root cause is confirmed but the fix requires a larger change (e.g. app-side fix).
- The entry in this file is filled in with root cause and expected fix date.
- The user has explicitly approved quarantine.

Quarantined tests must be revisited within **2 weeks** or escalated.
