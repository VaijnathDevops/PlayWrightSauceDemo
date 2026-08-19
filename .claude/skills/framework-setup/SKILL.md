---
name: framework-setup
description: Use when initializing or auditing the Playwright/NUnit project structure — multi-browser config, folder layout, base configuration, and sensible defaults. Run this before any test writing if the project structure is missing or incomplete.
---

# Framework Setup

Initializes or validates the C# Playwright/NUnit project structure with multi-browser support, correct folder layout, and sensible configuration defaults.

## Procedure

### 1. Audit the existing project

```powershell
# Check what already exists
Get-ChildItem TestProject1/ -Recurse | Select-Object FullName
```

- Read `TestProject1/TestProject1.csproj` — confirm package versions and target framework.
- Check for existing `Pages/`, `Tests/`, `Helpers/`, `TestData/` folders.
- Check for a `.runsettings` or `NUnit.runsettings` file at repo root.
- Check for `TestProject1/TestSettings.cs`, repo-root `.env.example`, `.env`, `.gitignore`, and `azure-pipelines.yml` — these are the credential-handling and CI pieces this skill also owns (steps 5b/5c).
- **Do not overwrite files that already exist and are correct** — extend, don't replace. Never overwrite an existing `.env` (it may hold a developer's real local values).

### 2. Validate / fix the `.csproj`

Required packages (read actual versions from `.csproj` — do not change unless user asks):
- `Microsoft.Playwright.NUnit` 1.52.0
- `NUnit` 4.3.2
- `NUnit3TestAdapter` 4.6.0
- `Microsoft.NET.Test.Sdk` 17.12.0
- `TargetFramework` net10.0

Run `dotnet restore` after any `.csproj` change.

### 3. Ensure browser binaries are installed

```powershell
# From TestProject1/ after build
$env:PLAYWRIGHT_BROWSERS_PATH = "0"
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium firefox
```

Confirm both `chromium` and `firefox` are installed. The skill targets **Chromium + Firefox** as the two required browsers.

### 4. Create the folder structure (only if missing)

```
TestProject1/
├── Pages/           # Page Object classes — one file per page/component
├── Tests/           # NUnit test fixtures — one file per feature
├── Helpers/         # Shared utilities: constants, base helpers, extensions
└── TestData/        # Structured test input: user lists, product data, checkout data
```

Create a `README.md` placeholder in each empty folder explaining its purpose so the intent is self-documenting:

- `Pages/README.md`
- `Tests/README.md`
- `Helpers/README.md`
- `TestData/README.md`

### 5. Create / validate `Helpers/SauceDemoConstants.cs`

**Non-sensitive constants only** — base URL and page paths. Credentials never go here, not even SauceDemo's public demo values; see step 5b.

```csharp
namespace TestProject1.Helpers
{
	public static class SauceDemoConstants
	{
		public const string BaseUrl  = "https://www.saucedemo.com/";

		public static class Urls
		{
			public const string Login            = "/";
			public const string Inventory        = "/inventory.html";
			public const string Cart             = "/cart.html";
			public const string CheckoutStepOne  = "/checkout-step-one.html";
			public const string CheckoutStepTwo  = "/checkout-step-two.html";
			public const string CheckoutComplete = "/checkout-complete.html";
		}
	}
}
```

### 5b. Create / validate credential loading (`TestSettings.cs`, `.env.example`, `.gitignore`)

Credentials (usernames, passwords) are never constants — they're read from environment variables via `TestProject1/TestSettings.cs`, backed by `.env` locally / Azure Pipelines secret variables in CI (see CLAUDE.md's "Credentials & Sensitive Data" section, and step 5c for the CI side). If any of the following are missing, create them — don't just flag the gap:

1. Add the `DotNetEnv` package. Resolve the real current version via the CLI rather than guessing one — if this repo also has a private/internal NuGet feed configured (check with `dotnet nuget list source`), pin explicitly to nuget.org so an unrelated private-feed auth failure doesn't block the install:
   ```powershell
   cd TestProject1
   dotnet add package DotNetEnv --source https://api.nuget.org/v3/index.json
   ```

2. Create `TestProject1/TestSettings.cs` (adjust the property names/env var names to whatever credentials this feature set actually needs — these three are SauceDemo's baseline):
   ```csharp
   namespace TestProject1
   {
       public static class TestSettings
       {
           static TestSettings()
           {
               // Walks up from the working directory (e.g. bin/Debug/net10.0 under `dotnet test`)
               // looking for a .env file. Never overwrites a variable already set in the real
               // environment, so Azure Pipelines secret variables always win over a stray .env.
               DotNetEnv.Env.TraversePath().Load();
           }

           public static string StandardUsername => Require("SAUCE_STANDARD_USERNAME");
           public static string LockedOutUsername => Require("SAUCE_LOCKED_OUT_USERNAME");
           public static string Password => Require("SAUCE_PASSWORD");

           private static string Require(string variableName)
           {
               var value = Environment.GetEnvironmentVariable(variableName);
               if (string.IsNullOrWhiteSpace(value))
               {
                   throw new InvalidOperationException(
                       $"Required environment variable '{variableName}' is not set. " +
                       "Set it in a local .env file (see .env.example) or as an Azure Pipelines secret variable.");
               }
               return value;
           }
       }
   }
   ```

3. Create repo-root `.env.example` (committed template — safe to share) with the same variable names. If `.env` doesn't already exist, also create it (git-ignored) with the same content so tests run locally out of the box:
   ```
   SAUCE_STANDARD_USERNAME=standard_user
   SAUCE_LOCKED_OUT_USERNAME=locked_out_user
   SAUCE_PASSWORD=secret_sauce
   ```
   Note in your report that these are SauceDemo's own published demo values and, per CLAUDE.md, should still be confirmed against the live site rather than trusted blindly.

4. Ensure repo-root `.gitignore` contains at least `.env`, `bin/`, `obj/`, `.vs/`, `*.user`, `TestResults/` — add missing entries to an existing file rather than replacing it; create it if absent.

### 5c. Create / validate the Azure DevOps CI pipeline (`azure-pipelines.yml`)

Create at repo root if missing (this exact YAML has already been verified in this repo — restore/build/browser-install/test/publish, credentials sourced from a variable group mapped to the same env var names as `.env.example`):

```yaml
trigger:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  # Create this variable group in Azure DevOps: Pipelines > Library > Variable groups.
  # Add these as SECRET variables (lock icon), matching .env.example's names:
  #   SAUCE_STANDARD_USERNAME, SAUCE_LOCKED_OUT_USERNAME, SAUCE_PASSWORD
  - group: sauce-demo-credentials

steps:
  - task: UseDotNet@2
    displayName: 'Use .NET SDK 10.x'
    inputs:
      packageType: sdk
      version: '10.0.x'

  - script: dotnet restore
    displayName: 'Restore dependencies'
    workingDirectory: TestProject1

  - script: dotnet build --configuration Release --no-restore
    displayName: 'Build'
    workingDirectory: TestProject1

  - script: pwsh bin/Release/net10.0/playwright.ps1 install --with-deps
    displayName: 'Install Playwright browsers'
    workingDirectory: TestProject1

  - script: dotnet test --configuration Release --no-build --logger trx --results-directory "$(Agent.TempDirectory)/TestResults"
    displayName: 'Run Playwright/NUnit tests'
    workingDirectory: TestProject1
    env:
      SAUCE_STANDARD_USERNAME: $(SAUCE_STANDARD_USERNAME)
      SAUCE_LOCKED_OUT_USERNAME: $(SAUCE_LOCKED_OUT_USERNAME)
      SAUCE_PASSWORD: $(SAUCE_PASSWORD)

  - task: PublishTestResults@2
    displayName: 'Publish test results'
    condition: succeededOrFailed()
    inputs:
      testResultsFormat: VSTest
      testResultsFiles: '$(Agent.TempDirectory)/TestResults/*.trx'
```

Notes:
- The browser-install line (`pwsh <output>/playwright.ps1 install --with-deps`) matches Playwright .NET's own documented Azure Pipelines example — keep the `net10.0`/`Release` path in sync with the `.csproj`'s `TargetFramework` and the build configuration used above.
- `sauce-demo-credentials` is a placeholder variable-group name — creating the group and marking its variables secret in Azure DevOps (Pipelines → Library) is a manual step outside this repo; call it out explicitly in your report rather than claiming CI is fully wired up.

### 6. Create / validate `.runsettings` for multi-browser runs

Create `TestProject1/playwright.runsettings` (or repo-root `playwright.runsettings`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <TestRunParameters>
	<!-- Override with: dotnet test -- TestRunParameters.Parameter\(name=\"browser\",value=\"firefox\"\) -->
	<Parameter name="browser"          value="chromium" />
	<Parameter name="headless"         value="true" />
	<Parameter name="slowMo"           value="0" />
	<Parameter name="timeout"          value="30000" />
	<Parameter name="navigationTimeout" value="30000" />
	<Parameter name="baseURL"          value="https://www.saucedemo.com/" />
	<Parameter name="retries"          value="1" />
	<Parameter name="video"            value="off" />
	<Parameter name="screenshot"       value="only-on-failure" />
	<Parameter name="trace"            value="retain-on-failure" />
  </TestRunParameters>
</RunSettings>
```

**How to run against a specific browser:**

```powershell
# Chromium (default)
dotnet test TestProject1/ --settings TestProject1/playwright.runsettings

# Firefox
dotnet test TestProject1/ --settings TestProject1/playwright.runsettings -- TestRunParameters.Parameter(name="browser",value="firefox")

# Both in one script (sequential)
foreach ($b in @("chromium","firefox")) {
	dotnet test TestProject1/ --settings TestProject1/playwright.runsettings `
		-- "TestRunParameters.Parameter(name=`"browser`",value=`"$b`")"
}
```

### 7. Create / validate `Helpers/PlaywrightFixture.cs` — shared browser config

```csharp
public class PlaywrightFixture : PageTest
{
	public override BrowserNewContextOptions ContextOptions() => new()
	{
		BaseURL      = SauceDemoConstants.BaseUrl,
		ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
	};
}
```

All test fixtures inherit `PlaywrightFixture` (not `PageTest` directly).

### 8. Build and verify

```powershell
cd TestProject1
dotnet build
dotnet test --list-tests
```

- Build must succeed with 0 errors.
- `--list-tests` must enumerate at least one test (the existing `UnitTest1` scaffold is fine at this stage).
- Fix any compile errors before reporting done.

### 9. Report

Summarise what was created, what already existed (and was left unchanged), and how to run against each browser. Explicitly call out: (a) credential files created (`TestSettings.cs`, `.env.example`, `.env`, `.gitignore`) and that `.env`'s values are unverified SauceDemo defaults, and (b) that `azure-pipelines.yml` was created but the `sauce-demo-credentials` variable group must still be created manually in Azure DevOps before CI will actually pass. Point the user to `/create-page-objects` as the next step.

## Anti-hallucination

- Do not invent package versions — read `TestProject1.csproj` for what is actually installed, and let `dotnet add package` resolve `DotNetEnv`'s version rather than hardcoding one.
- Do not claim the build passes without running `dotnet build`.
- Do not claim browsers are installed without running `playwright.ps1 install` or confirming the output.
- Do not claim CI "is set up" — the YAML file existing is not the same as the pipeline being able to pass; the Azure DevOps variable group is a manual step this skill cannot perform, so say so.
