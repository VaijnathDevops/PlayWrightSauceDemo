---
name: page-object-model-generator
description: Use when creating or extending Page Object classes for the application under test. Produces well-structured C# POM files in TestProject1/Pages/ using Playwright's preferred locator strategies (role, label, text, testid over CSS/XPath).
---

# Page Object Model Generator

Creates or extends Page Object classes in `TestProject1/Pages/`, one class per page/component, following the POM conventions in this repo.

---

## Procedure

### 1. Identify which pages are needed

- Read the requirement or list of pages given by the user.
- Glob `TestProject1/Pages/*.cs` — if a class for a page already exists, **extend it**, don't create a duplicate.
- For each page not yet covered, create `TestProject1/Pages/<PageName>Page.cs`.
- Minimum three distinct pages must be covered by this skill run (per project requirement). For SauceDemo, the standard set is: `LoginPage`, `InventoryPage`, `CartPage`, `CheckoutPage`, `ProductDetailPage`.

---

### 2. Inspect the real app for selectors (never invent them)

For each page, navigate to it and read the actual DOM:

```powershell
npx playwright codegen https://www.saucedemo.com/
```

Or run a throwaway snippet:

```csharp
await Page.GotoAsync("https://www.saucedemo.com/");
Console.WriteLine(await Page.ContentAsync());
```

**Before using any `GetByTestId(...)` call below**: it matches Playwright's default `data-testid` attribute, not SauceDemo's `data-test` attribute. It only works once `Selectors.SetTestIdAttribute("data-test")` has been configured — confirm that's actually wired up in this project (build + run a test that depends on it) before trusting these; if it isn't wired up, use `Page.Locator("[data-test='...']")` instead for these rows and say so in your report.

Known `data-test` attributes for SauceDemo (labeled "verified" by a prior session — spot-check at least one against the live app yourself this session per this repo's anti-hallucination policy rather than trusting the label blindly):

| Page | Element | Preferred Locator |
|---|---|---|
| Login | Username field | `Page.GetByTestId("username")` |
| Login | Password field | `Page.GetByTestId("password")` |
| Login | Login button | `Page.GetByRole(AriaRole.Button, new() { Name = "Login" })` |
| Login | Error message | `Page.GetByTestId("error")` |
| Inventory | Product items | `Page.Locator(".inventory_item")` |
| Inventory | Sort dropdown | `Page.GetByTestId("product-sort-container")` |
| Inventory | Cart icon | `Page.GetByTestId("shopping-cart-link")` |
| Inventory | Cart badge | `Page.GetByTestId("shopping-cart-badge")` |
| Inventory | Add to cart (by product name) | `Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First` |
| Cart | Cart items | `Page.Locator(".cart_item")` |
| Cart | Checkout button | `Page.GetByTestId("checkout")` |
| Cart | Continue shopping | `Page.GetByTestId("continue-shopping")` |
| Cart | Remove button | `Page.GetByRole(AriaRole.Button, new() { Name = "Remove" })` |
| Checkout Step 1 | First name | `Page.GetByTestId("firstName")` |
| Checkout Step 1 | Last name | `Page.GetByTestId("lastName")` |
| Checkout Step 1 | Postal code | `Page.GetByTestId("postalCode")` |
| Checkout Step 1 | Continue | `Page.GetByTestId("continue")` |
| Checkout Step 1 | Error message | `Page.GetByTestId("error")` |
| Checkout Step 2 | Summary total | `Page.GetByTestId("total-label")` |
| Checkout Step 2 | Finish button | `Page.GetByTestId("finish")` |
| Checkout Complete | Header | `Page.GetByTestId("complete-header")` |
| Checkout Complete | Back to products | `Page.GetByTestId("back-to-products")` |

**Locator priority (must follow this order — don't skip to a lower tier just because it's simpler to write or the element happens to have an `id`):**
1. `GetByRole` — semantic, most resilient (works for `<button>`, `<input type="submit">`, links, etc. via implicit ARIA role)
2. `GetByLabel` — for form inputs with a real associated `<label>`
3. `GetByText` — for buttons/links identified by visible text
4. `GetByPlaceholder` — for inputs with no `<label>` but a visible placeholder (SauceDemo's login fields are typically placeholder-only, not `<label>`-associated — check before assuming `GetByLabel` applies)
5. `GetByTestId` — for elements with `data-test` attributes, **only once `Selectors.SetTestIdAttribute("data-test")` is confirmed wired up** (see note above)
6. CSS selector (e.g. `Page.Locator("[data-test='...']")` or `#id`) — last resort only, and only when none of the above apply

---

### 3. Page Object structure rules

```csharp
using Microsoft.Playwright;

namespace TestProject1.Pages
{
	public class <PageName>Page(IPage page)
	{
		private readonly IPage _page = page;

		// ── Locators ── (private, lazy properties — not fields)
		private ILocator <ElementName> => _page.Get...(...);

		// ── Navigation ──
		public async Task GoToAsync() =>
			await _page.GotoAsync(Helpers.SauceDemoConstants.Urls.<Page>);

		// ── Actions ── (async Task or async Task<T>)
		public async Task <ActionName>Async(...) { ... }

		// ── Queries ── (async Task<T> — read state without side-effects)
		public async Task<string> Get<Property>Async() => await <locator>.<ReadMethod>();
		public async Task<bool>   Is<State>Async()     => await <locator>.IsVisibleAsync();
	}
}
```

**Mandatory rules:**
- Constructor uses C# 12 primary constructor syntax `(IPage page)`.
- Locators are `private ILocator` **properties** (not fields) — evaluated each time, avoids stale references.
- **No assertions inside Page Objects** — assertions belong in test fixtures only.
- **No navigation inside action methods** (except `GoToAsync`) — let the test control flow.
- All public methods are `async Task` or `async Task<T>`.
- Use `SauceDemoConstants.Urls.*` for URLs — no hardcoded strings.
- **Never hardcode a credential value** (username, password, token) in a Page Object, including in `SauceDemoConstants` — not even SauceDemo's own public demo accounts. Actions like `LoginAsync` accept credentials as parameters (as shown below); the caller supplies them from `TestSettings` (`TestProject1/TestSettings.cs`). See CLAUDE.md's "Credentials & Sensitive Data" section.

---

### 4. Canonical Page Object implementations

#### `LoginPage.cs`

```csharp
using Microsoft.Playwright;
using TestProject1.Helpers;

namespace TestProject1.Pages
{
	public class LoginPage(IPage page)
	{
		private readonly IPage _page = page;

		private ILocator UsernameInput => _page.GetByTestId("username");
		private ILocator PasswordInput => _page.GetByTestId("password");
		private ILocator LoginButton   => _page.GetByRole(AriaRole.Button, new() { Name = "Login" });
		private ILocator ErrorBanner   => _page.GetByTestId("error");

		public async Task GoToAsync() =>
			await _page.GotoAsync(SauceDemoConstants.BaseUrl);

		public async Task LoginAsync(string username, string password)
		{
			await UsernameInput.FillAsync(username);
			await PasswordInput.FillAsync(password);
			await LoginButton.ClickAsync();
		}

		public async Task<string> GetErrorMessageAsync() =>
			await ErrorBanner.InnerTextAsync();

		public async Task<bool> IsErrorVisibleAsync() =>
			await ErrorBanner.IsVisibleAsync();
	}
}
```

#### `InventoryPage.cs`

```csharp
using Microsoft.Playwright;
using TestProject1.Helpers;

namespace TestProject1.Pages
{
	public class InventoryPage(IPage page)
	{
		private readonly IPage _page = page;

		private ILocator ProductItems   => _page.Locator(".inventory_item");
		private ILocator SortDropdown   => _page.GetByTestId("product-sort-container");
		private ILocator CartIcon       => _page.GetByTestId("shopping-cart-link");
		private ILocator CartBadge      => _page.GetByTestId("shopping-cart-badge");
		private ILocator BurgerMenu     => _page.GetByRole(AriaRole.Button, new() { Name = "Open Menu" });

		public async Task GoToAsync() =>
			await _page.GotoAsync(SauceDemoConstants.BaseUrl + SauceDemoConstants.Urls.Inventory.TrimStart('/'));

		public async Task AddToCartByNameAsync(string productName)
		{
			var item = _page.Locator(".inventory_item").Filter(new() { HasText = productName });
			await item.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();
		}

		public async Task RemoveFromCartByNameAsync(string productName)
		{
			var item = _page.Locator(".inventory_item").Filter(new() { HasText = productName });
			await item.GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
		}

		public async Task SortByAsync(string sortValue) =>
			await SortDropdown.SelectOptionAsync(sortValue);

		public async Task GoToCartAsync() =>
			await CartIcon.ClickAsync();

		public async Task<int> GetProductCountAsync() =>
			await ProductItems.CountAsync();

		public async Task<string> GetCartBadgeCountAsync() =>
			await CartBadge.InnerTextAsync();

		public async Task<bool> IsCartBadgeVisibleAsync() =>
			await CartBadge.IsVisibleAsync();

		public async Task<List<string>> GetProductNamesAsync()
		{
			var names = new List<string>();
			var count = await ProductItems.CountAsync();
			for (var i = 0; i < count; i++)
				names.Add(await ProductItems.Nth(i).Locator(".inventory_item_name").InnerTextAsync());
			return names;
		}
	}
}
```

#### `CartPage.cs`

```csharp
using Microsoft.Playwright;
using TestProject1.Helpers;

namespace TestProject1.Pages
{
	public class CartPage(IPage page)
	{
		private readonly IPage _page = page;

		private ILocator CartItems         => _page.Locator(".cart_item");
		private ILocator CheckoutButton    => _page.GetByTestId("checkout");
		private ILocator ContinueShopping  => _page.GetByTestId("continue-shopping");

		public async Task GoToAsync() =>
			await _page.GotoAsync(SauceDemoConstants.BaseUrl + SauceDemoConstants.Urls.Cart.TrimStart('/'));

		public async Task ProceedToCheckoutAsync() =>
			await CheckoutButton.ClickAsync();

		public async Task ContinueShoppingAsync() =>
			await ContinueShopping.ClickAsync();

		public async Task RemoveItemByNameAsync(string productName)
		{
			var item = CartItems.Filter(new() { HasText = productName });
			await item.GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
		}

		public async Task<int> GetItemCountAsync() =>
			await CartItems.CountAsync();

		public async Task<bool> IsItemInCartAsync(string productName) =>
			await CartItems.Filter(new() { HasText = productName }).IsVisibleAsync();
	}
}
```

#### `CheckoutPage.cs`

```csharp
using Microsoft.Playwright;
using TestProject1.Helpers;

namespace TestProject1.Pages
{
	public class CheckoutPage(IPage page)
	{
		private readonly IPage _page = page;

		// Step 1
		private ILocator FirstNameInput => _page.GetByTestId("firstName");
		private ILocator LastNameInput  => _page.GetByTestId("lastName");
		private ILocator PostalCodeInput => _page.GetByTestId("postalCode");
		private ILocator ContinueButton  => _page.GetByTestId("continue");
		private ILocator StepOneError    => _page.GetByTestId("error");

		// Step 2
		private ILocator FinishButton    => _page.GetByTestId("finish");
		private ILocator SummaryTotal    => _page.GetByTestId("total-label");

		// Complete
		private ILocator CompleteHeader     => _page.GetByTestId("complete-header");
		private ILocator BackToProductsBtn  => _page.GetByTestId("back-to-products");

		public async Task FillCustomerInfoAsync(string firstName, string lastName, string postalCode)
		{
			await FirstNameInput.FillAsync(firstName);
			await LastNameInput.FillAsync(lastName);
			await PostalCodeInput.FillAsync(postalCode);
		}

		public async Task ContinueAsync() =>
			await ContinueButton.ClickAsync();

		public async Task FinishAsync() =>
			await FinishButton.ClickAsync();

		public async Task BackToProductsAsync() =>
			await BackToProductsBtn.ClickAsync();

		public async Task<string> GetStepOneErrorAsync() =>
			await StepOneError.InnerTextAsync();

		public async Task<bool> IsStepOneErrorVisibleAsync() =>
			await StepOneError.IsVisibleAsync();

		public async Task<string> GetOrderTotalAsync() =>
			await SummaryTotal.InnerTextAsync();

		public async Task<string> GetCompleteHeaderAsync() =>
			await CompleteHeader.InnerTextAsync();

		public async Task<bool> IsOrderCompleteAsync() =>
			await CompleteHeader.IsVisibleAsync();
	}
}
```

---

### 5. Build after every file

```powershell
cd TestProject1
dotnet build
```

Fix any compile errors before writing the next file. Do not leave the project in a broken state between Page Object creations.

---

### 6. Final verification

```powershell
dotnet build
dotnet test --list-tests
```

- 0 compile errors.
- All previously passing tests still listed.

---

### 7. Report and hand off

List all Page Objects created/extended. For each one, show:
- File path
- Public methods (name + signature)
- Locators added

End with: "Next: run `/automate-tests TestCases/<Feature>.md` to generate test fixtures that use these Page Objects."

---

## Anti-hallucination

- Only use selectors from the verified table above or ones you have inspected on the live app this session.
- Only use Playwright `ILocator` methods that exist in `Microsoft.Playwright` 1.52.0.
- Do not claim the build passes without running `dotnet build`.
