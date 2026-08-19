---
description: Create or extend Page Object classes for the application under test in SauceAppTests/Pages/
---

Use the `pom-engineer` agent to create or extend Page Object classes in `SauceAppTests/Pages/`, following the `page-object-model-generator` skill and the project `CLAUDE.md` conventions.

The agent will:
1. Audit `SauceAppTests/Pages/` — extend existing classes rather than creating duplicates
2. Resolve selectors from the verified SauceDemo selector table or by inspecting the live app — never invents locators
3. Follow locator priority: `GetByRole` → `GetByLabel` → `GetByText` → `GetByTestId` → CSS last resort
4. Create well-structured POM classes (primary constructor, private locator properties, no assertions inside POMs)
5. Use `SauceDemoConstants` for all URLs and credentials
6. Build with `dotnet build` after every file — fix compile errors before moving on
7. Report all files created/extended with their public method signatures

Pages to create / extend (list page names, or leave blank for the full SauceDemo standard set: Login, Inventory, Cart, Checkout):
$ARGUMENTS
