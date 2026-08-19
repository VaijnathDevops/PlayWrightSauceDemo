---
description: Initialize or audit the Playwright/NUnit project structure — multi-browser config, folder layout, base configuration, and sensible defaults
---

Use the `framework-setup-engineer` agent to set up or validate the Playwright/NUnit test framework for this project, following the `framework-setup` skill and the project `CLAUDE.md` guidelines.

The agent will:
1. Audit the existing project structure (never overwrites correct files)
2. Validate / fix `TestProject1.csproj` package references
3. Install Chromium + Firefox browser binaries via `playwright.ps1 install`
4. Create missing folders: `Pages/`, `Tests/`, `Helpers/`, `TestData/`
5. Create `Helpers/SauceDemoConstants.cs` with non-sensitive shared constants (base URL, page paths — never credentials)
6. Set up credential loading: `TestSettings.cs`, `.env.example`/`.env`, `.gitignore` entries
7. Create `azure-pipelines.yml` (Azure DevOps CI: build, install browsers, test, publish results) wired to the same credential env var names
8. Create `playwright.runsettings` with multi-browser + timeout + reporter defaults
9. Create `Helpers/PlaywrightFixture.cs` as the shared base class for all test fixtures
10. Build with `dotnet build` and verify with `dotnet test --list-tests`
11. Report what was created vs already existed, the browser-specific run commands, and the manual Azure DevOps variable-group step still required for CI

Scope / notes (optional — describe what's missing or what you want customised):
$ARGUMENTS
