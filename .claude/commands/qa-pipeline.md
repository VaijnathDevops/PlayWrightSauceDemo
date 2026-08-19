---
description: Full pipeline — requirement to manual test cases to automation to a green suite
---

Run the full QA pipeline described in `.claude/workflows/testing-workflow.md` for the requirement below, end to end:

1. `qa-test-designer` → write manual test cases to `TestCases/<Feature>.md`.
2. `playwright-automation-engineer` → generate automation from that file into `SauceAppTests/`.
3. `test-healer` → run the new tests and fix any failures until green or a genuine blocker is found.

Report a short summary at the end: test case file written, automation files created/changed, final suite status.

Requirement:
$ARGUMENTS
