---
name: manual-test-case-writer
description: Use when turning a feature description, user story, or acceptance criteria into a manual QA test case document for this repo, before any automation is written. Produces one Markdown file under TestCases/.
---

# Manual Test Case Writer

Turns a requirement into a single Markdown test case file: `TestCases/<Feature>.md`.

## Procedure

1. **Gather context.**
   - Read the requirement as given. List explicit acceptance criteria if any exist.
   - Glob `TestCases/*.md` — if a file for this feature already exists, extend/update it rather than creating a duplicate.
   - Glob/Grep `TestProject1/Pages` and `TestProject1/Tests` — existing Page Objects and tests reveal real field names, flows, and selectors already in use; reuse that terminology.
   - If the requirement doesn't name the target app/page, default to SauceDemo (https://www.saucedemo.com) per this repo's purpose, and say so.

2. **Design the coverage matrix.** For the feature, enumerate:
   - **Positive**: each valid path to the goal (including any valid alternate paths).
   - **Negative**: invalid input, wrong credentials, missing required fields, unauthorized access attempts.
   - **Boundary/edge**: empty input, max-length input, special characters, zero/one/many for lists or carts, network/slow-load if relevant.
   - **State-dependent**: behavior differs when logged in vs out, cart empty vs full, etc. — include both states if the feature touches them.
   - Skip cases that don't add distinct coverage (don't pad the matrix for its own sake).

3. **Write the file** using the template below. One file per feature; if the requirement spans multiple features, split accordingly and say so.

## File template

```markdown
# Test Cases: <Feature Name>

**Source requirement:** <one-line summary or link/quote of what was given>
**Target app:** <e.g. https://www.saucedemo.com>
**Assumptions:** <anything inferred rather than stated explicitly — omit section if none>

| ID | Title | Priority | Type | Preconditions | Steps | Test Data | Expected Result |
|----|-------|----------|------|----------------|-------|-----------|------------------|
| TC-LOGIN-001 | Valid login succeeds | High | Positive | User is on the login page | 1. Enter valid username 2. Enter valid password 3. Click Login | Standard-user credentials (`TestSettings.StandardUsername` / `.Password`, see `.env.example`) | User is redirected to the inventory page |
| TC-LOGIN-002 | Login fails with locked-out user | High | Negative | User is on the login page | 1. Enter locked-out user's credentials 2. Click Login | Locked-out-user credentials (`TestSettings.LockedOutUsername` / `.Password`) | Error message indicating the user has been locked out is shown; user stays on login page |
```

- **ID scheme**: `TC-<FEATURE-SLUG>-<3-digit sequence>`, e.g. `TC-CHECKOUT-004`. Feature slug is short, uppercase, no spaces.
- **Priority**: High / Medium / Low, based on user impact.
- **Type**: Positive / Negative / Boundary / State / Data-Driven. Use **Data-Driven** for a single row whose Test Data column lists multiple parameter sets each mapped to its own expected result (e.g. several credential pairs, each with its own outcome) — it's meant to become one parameterized `[TestCase]`-driven automated test rather than several near-duplicate rows.
- Keep **Steps** numbered and atomic (one user action per step) so they map cleanly to future automation steps.
- **Test Data** must be precise about *which* data is used, but must **never contain a literal credential value** (username, password, token, API key) — not even SauceDemo's public demo values. Reference credentials by role and point to their source instead, per CLAUDE.md's "Credentials & Sensitive Data" section — e.g. "Standard-user credentials (`TestSettings.StandardUsername` / `.Password`)" or "an incorrect password (any value that does not equal `TestSettings.Password`)". Non-sensitive data (item names, quantities, zip codes, etc.) should stay concrete and literal.
- **Expected Result** must be objectively verifiable (specific text, URL, element state) — not "works correctly".

## Anti-hallucination

Don't state exact UI copy (button text, error messages) unless it came from the requirement, existing code, or a source actually inspected this session. If unknown, phrase the expected result around the observable behavior and mark the literal copy `[TBD: confirm exact text]`.

## Handoff

End with: "Next: run `/automate-tests TestCases/<Feature>.md` to generate the Playwright automation for these cases."
