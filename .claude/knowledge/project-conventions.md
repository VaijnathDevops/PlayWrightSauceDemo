# Project Conventions & Patterns

Coding standards and architectural patterns for **PlayWrightSauceDemo** (`TestProject1`). These supplement the root `CLAUDE.md` guidelines.

## Directory Layout

```
TestProject1/
├── Pages/                  # Page Object classes (one per page/component)
│   ├── LoginPage.cs
│   ├── InventoryPage.cs
│   └── ...
├── Tests/                  # NUnit test fixtures (one per feature)
│   ├── LoginTests.cs
│   └── ...
├── Helpers/                # Shared utilities (test data builders, constants, etc.)
│   └── SauceDemoConstants.cs
└── UnitTest1.cs            # Original scaffold sample — do not add tests here
```

`TestCases/` lives at the repo root, outside `TestProject1/`.

## Page Object Pattern

```csharp
// TestProject1/Pages/LoginPage.cs
using Microsoft.Playwright;

namespace TestProject1.Pages
{
	public class LoginPage(IPage page)
	{
		private readonly IPage _page = page;

		// Locators — defined once, reused everywhere
		private ILocator UsernameInput  => _page.GetByTestId("username");
		private ILocator PasswordInput  => _page.GetByTestId("password");
		private ILocator LoginButton    => _page.GetByRole(AriaRole.Button, new() { Name = "Login" });
		private ILocator ErrorMessage   => _page.GetByTestId("error");

		public async Task GoToAsync() =>
			await _page.GotoAsync("https://www.saucedemo.com/");

		public async Task LoginAsync(string username, string password)
		{
			await UsernameInput.FillAsync(username);
			await PasswordInput.FillAsync(password);
			await LoginButton.ClickAsync();
		}

		public async Task<string> GetErrorMessageAsync() =>
			await ErrorMessage.InnerTextAsync();
	}
}
```

**Rules:**
- Locators are `private` properties (not fields) — evaluated lazily, not at construction time.
- Methods are `async Task` / `async Task<T>`.
- No assertions inside Page Objects — assertions belong in the test.

## Test Fixture Pattern

```csharp
// TestProject1/Tests/LoginTests.cs
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using TestProject1.Pages;

namespace TestProject1
{
	[TestFixture]
	public class LoginTests : PageTest
	{
		private LoginPage _loginPage = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_loginPage = new LoginPage(Page);
			await _loginPage.GoToAsync();
		}

		[Test]
		[Description("TC-LOGIN-001")]
		public async Task Login_WithValidCredentials_NavigatesToInventory()
		{
			await _loginPage.LoginAsync("standard_user", "secret_sauce");
			await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/inventory.html");
		}

		[Test]
		[Description("TC-LOGIN-002")]
		[TestCase("locked_out_user", "secret_sauce", "Sorry, this user has been locked out")]
		[TestCase("", "secret_sauce", "Username is required")]
		[TestCase("standard_user", "", "Password is required")]
		public async Task Login_WithInvalidInput_ShowsExpectedError(
			string username, string password, string expectedError)
		{
			await _loginPage.LoginAsync(username, password);
			await Expect(Page.GetByTestId("error")).ToContainTextAsync(expectedError);
		}
	}
}
```

## Naming Conventions

| Item | Convention | Example |
|---|---|---|
| Test method | `Subject_Condition_ExpectedResult` | `Login_WithLockedUser_ShowsLockoutError` |
| Page Object class | `<Page>Page` | `LoginPage`, `InventoryPage` |
| Test fixture class | `<Feature>Tests` | `LoginTests`, `CheckoutTests` |
| Test case file | `TestCases/<Feature>.md` | `TestCases/Login.md` |
| Test case ID | `TC-<FEATURE>-<NUM>` | `TC-LOGIN-001` |
