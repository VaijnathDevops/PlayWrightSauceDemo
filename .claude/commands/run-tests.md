---
description: Run all tests, or one test/class by name; auto-heal any failures and refresh reports/*.md for the run
---

Run the suite following the `test-execution` skill (`.claude/skills/test-execution/SKILL.md`):

1. `test-runner` agent → execute (scoped to the argument below if given, otherwise the full suite).
2. If anything failed, `test-healer` agent → diagnose and fix (≤5 iterations per test, per the project's Self-Healing Policy).
3. `test-runner` agent → confirmation re-run over the same scope.
4. Update `reports/execution-report.md`, `reports/healing-report.md`, `reports/coverage-report.md`, and `reports/flaky-tests.md` for this execution.

Report final pass/fail/skip counts, what was healed (and how), and which report files changed.

Test name or class to scope to (optional — leave empty to run the full suite):
$ARGUMENTS
