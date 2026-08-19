# Test Healing Report

Tracks every self-heal session: which tests were failing, what was tried, and the final outcome. Updated by the `test-healer` agent or manually after a `/heal-tests` run.

---

## Healing Log

<!-- Add a new block below for each healing session. Most recent first. -->

### Session — YYYY-MM-DD

| Field | Value |
|---|---|
| Date | YYYY-MM-DD |
| Trigger | `/heal-tests` / manual |
| Scope | Full suite / `<ClassName>` / `<MethodName>` |
| Iterations cap | 5 per test |

#### Outcome Table

| Test | Iterations | Outcome | Root Cause | Change Made |
|---|---|---|---|---|
| — | — | — | — | — |

**Outcome key:**
- ✅ Fixed — test is now green
- ❌ Still failing — escalated to user
- 🐛 App bug — filed as `BUG-<NUM>` in `qa-assets/bug-reports/`
- 🚫 Stopped — hit 5-iteration cap

#### Session Notes

---

## Policy Reminders (Self-Healing Policy)

- Never weaken assertions, add `Thread.Sleep`, or add `[Ignore]`/`[Explicit]` without explicit user sign-off.
- Stop at **5 iterations per test** and escalate with root-cause analysis.
- If the app is broken (not the test), file a bug report and mark outcome 🐛.
- If a fix changes what the test verifies, update the corresponding `TestCases/<Feature>.md`.
