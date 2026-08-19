---
description: Diagnose and fix failing tests, iterating until green or a genuine blocker is found
---

Use the `test-healer` agent to run the test suite and iteratively diagnose/fix failures, following the `test-self-heal` skill and the "Self-Healing Policy" in the project `CLAUDE.md`. Cap at 5 fix attempts per test. Report a final table of test → outcome → root cause → change made.

Scope (optional — a test name, class, or file to focus on; if empty, run the full suite):
$ARGUMENTS
