---
name: test-execution
description: Use when the user wants to run the Playwright/NUnit suite (all tests, or one by name) and have any failures auto-healed and every report under reports/ refreshed for the run, in one pass.
---

# Test Execution Loop

Runs the suite (scoped or full), heals failures, and brings all `reports/*.md` files up to date for the run — one flow covering execute → heal → report.

## Scope resolution

- If a test name, class name, or filter keyword is supplied, scope the run: `dotnet test --filter "FullyQualifiedName~<name>"`.
- If nothing is supplied, run the full suite: `dotnet test`.
- A "test name" can be a full method name, a partial match, or a fixture/class name — pass it through to `--filter` as-is; `dotnet test`'s filtering handles partial matches.

## Procedure

1. **Execute.** Use the `test-runner` agent to run the suite per the scope above. It writes the run's raw results into `reports/execution-report.md`.
2. **Branch on result.**
   - All green → skip to step 5.
   - Any failures → step 3.
3. **Heal.** Use the `test-healer` agent (the `test-self-heal` skill, capped at 5 iterations per test) to diagnose and fix the failing tests. Don't touch assertions or reports yourself in this step — that agent owns the fix loop and its own iteration log.
4. **Re-run to confirm.** After healing finishes (fixed, still-red, or app-bug), use the `test-runner` agent again for a confirmation run over the same scope, so `reports/execution-report.md`'s latest block reflects the *actual current* state, not the pre-heal state.
5. **Update every report** for this execution:
   - `reports/execution-report.md` — final run block (written in step 1, and again in step 4 if healing ran).
   - `reports/healing-report.md` — new Session block if step 3 ran, following its existing format (Outcome Table with ✅ / ❌ / 🐛 / 🚫 per test). Skip this file if nothing needed healing.
   - `reports/coverage-report.md` — re-sync the coverage matrix if the run touched TC IDs not yet reflected there (new automated tests, renamed methods). Use the audit commands in that file's "How to Audit Coverage" section.
   - `reports/flaky-tests.md` — add or update a row only for a test that failed in step 1, was **not** touched by a code change in step 3, and passed anyway on the step 4 re-run. A test fixed by an actual code change in step 3 is not flaky — don't log it there.
6. **Report to the user**: final pass/fail/skip counts, which tests were healed and how, any app-bug findings, and which report files were updated.

## Rules

- Never report a result without having actually run `dotnet test` this session — no predicted or assumed outcomes.
- Healing follows the Self-Healing Policy in the root `CLAUDE.md` and the `test-self-heal` skill exactly: no weakened assertions, no `Thread.Sleep`, no `[Ignore]`/`[Explicit]`, 5-iteration cap per test, stop and report on a genuine app bug rather than forcing green.
- If the confirmation re-run (step 4) still shows failures after healing hit its cap, report that plainly in both the summary and `reports/healing-report.md` — don't mark it ✅.
- Report updates are additive (a new block at the top of a log, or a new/updated row in a matrix) — never delete prior run history from these files.
