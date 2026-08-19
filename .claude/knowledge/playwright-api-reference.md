# Playwright .NET — API Quick Reference

Versions: **Microsoft.Playwright 1.52.0**, **NUnit 4.3.2**, **.NET 10**

## Navigation

```csharp
await Page.GotoAsync("https://www.saucedemo.com/");
await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });
await Page.ReloadAsync();
await Page.GoBackAsync();
```

## Locators

```csharp
// Preferred — role/label/text/testid
Page.GetByRole(AriaRole.Button, new() { Name = "Login" });
Page.GetByLabel("Username");
Page.GetByText("Sauce Labs Backpack");
Page.GetByTestId("add-to-cart");         // data-test="add-to-cart"
Page.GetByPlaceholder("Username");

// CSS fallback
Page.Locator("[data-test='username']");
Page.Locator(".inventory_item").First;
Page.Locator(".inventory_item").Nth(2);

// Chaining
Page.Locator(".cart_item").GetByRole(AriaRole.Button, new() { Name = "Remove" });
```

## Actions

```csharp
await locator.ClickAsync();
await locator.FillAsync("standard_user");
await locator.SelectOptionAsync("az");    // <select> by value
await locator.CheckAsync();
await locator.PressAsync("Enter");
await locator.ClearAsync();
```

## Assertions

```csharp
await Expect(locator).ToBeVisibleAsync();
await Expect(locator).ToBeHiddenAsync();
await Expect(locator).ToBeEnabledAsync();
await Expect(locator).ToBeDisabledAsync();
await Expect(locator).ToHaveTextAsync("exact text");
await Expect(locator).ToContainTextAsync("partial");
await Expect(locator).ToHaveValueAsync("field value");
await Expect(locator).ToHaveCountAsync(6);
await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/inventory.html");
await Expect(Page).ToHaveTitleAsync("Swag Labs");
```

## Waiting — use assertions, never delays

```csharp
await Expect(locator).ToBeVisibleAsync(); // auto-wait
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
// BAD: Task.Delay / Thread.Sleep
```
