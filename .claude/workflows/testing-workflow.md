# QA Workflow: Requirement → Manual Test Cases → Automation → Self-Heal

End-to-end flow for turning a test requirement into a passing automated Playwright suite in this repo.

```
 Requirement (user prompt / user story / acceptance criteria)
        │
        ▼
 ┌───────────────────────────┐
 │ /write-test-cases         │   agent: qa-test-designer
 │ skill: manual-test-case-  │   → TestCases/<Feature>.md
 │        writer             │     (single Markdown file, positive/
 └────────────┬──────────────┘      negative/boundary/state cases)
              │
              ▼
 ┌───────────────────────────┐
 │ /automate-tests           │   agent: playwright-automation-engineer
 │ skill: playwright-         │   → SauceAppTests/Pages/*.cs
 │        automation-        │     SauceAppTests/Tests/*.cs
 │        generator           │   (builds + runs once, reports status)
 └────────────┬──────────────┘
              │
              ▼
        dotnet test
              │
      ┌───────┴────────┐
      │                │
   all green        failures
      │                │
      ▼                ▼
    done      ┌───────────────────────────┐
              │ /heal-tests               │   agent: test-healer
              │ skill: test-self-heal     │   diagnose → fix → rerun,
              └────────────┬──────────────┘   ≤5 iterations per test
                            │
                    ┌───────┴────────┐
                    │                │
                 green         still red after 5
                    │                │
                    ▼                ▼
                  done      stop, report root cause,
                            hand decision to user
```

`/qa-pipeline "<requirement>"` runs all three stages back to back for a fresh requirement.

## Stage contracts

- **qa-test-designer** never writes code. Its only output is `TestCases/<Feature>.md`.
- **playwright-automation-engineer** never invents selectors — it inspects the real app or asks. It must build and run what it writes before calling the task done.
- **test-healer** never fakes a pass — no weakened assertions, no `Thread.Sleep`, no `[Ignore]`, without explicit user sign-off. It stops and reports rather than looping forever.

## Traceability

Every manual test case ID (`TC-<FEATURE>-<NUM>`) written by `qa-test-designer` should end up referenced in a `[Description("TC-...")]` on the `[Test]` method that automates it, so coverage can be audited by grepping for the ID in both `TestCases/` and `SauceAppTests/Tests/`.

## Guardrails

All three stages follow the shared "Code Guidelines", "Anti-Hallucination Rules", and "Self-Healing Policy" sections in the root `CLAUDE.md` — that file is the source of truth if anything here seems to conflict with it.
