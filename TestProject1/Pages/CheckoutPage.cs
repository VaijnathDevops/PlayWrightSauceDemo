using Microsoft.Playwright;

namespace TestProject1.Pages
{
    /// <summary>
    /// Page Object for SauceDemo's multi-step checkout flow:
    /// - Step One / customer info: https://www.saucedemo.com/checkout-step-one.html
    /// - Step Two / order overview: https://www.saucedemo.com/checkout-step-two.html
    /// - Complete / confirmation: https://www.saucedemo.com/checkout-complete.html
    /// Selectors verified against the live app on 2026-08-18.
    ///
    /// Locator tier notes (see CLAUDE.md selector priority order):
    /// - First Name / Last Name / Zip/Postal Code inputs have no associated &lt;label for="..."&gt; (same
    ///   situation as LoginPage's username/password fields), but do have real placeholder text
    ///   (placeholder="First Name" / "Last Name" / "Zip/Postal Code"), so GetByPlaceholder is the correct
    ///   highest-available tier. Confirmed live: each resolves to exactly one element.
    /// - "Continue" and "Finish" are plain-text &lt;button&gt;s with implicit "button" accessible roles and
    ///   unique names on their respective pages — confirmed live — so GetByRole(Button, ...) applies.
    /// - The order-complete heading is &lt;h2 class="complete-header" data-test="complete-header"&gt;Thank you
    ///   for your order!&lt;/h2&gt; — a real &lt;h2&gt; has an implicit "heading" accessible role, confirmed live via
    ///   AriaSnapshotAsync, so GetByRole(Heading, "Thank you for your order!") is used.
    /// - The validation error banner is &lt;h3 data-test="error"&gt;...&lt;/h3&gt;, same situation as
    ///   LoginPage.ErrorMessage: no role="alert"/aria-live and message content varies per scenario, so
    ///   GetByRole/GetByText aren't viable and the raw CSS attribute selector is the documented last
    ///   resort for this element only.
    /// - The page sub-header (e.g. "Checkout: Your Information", "Checkout: Overview",
    ///   "Checkout: Complete!") reuses the same &lt;[data-test='title']&gt; element already established as
    ///   InventoryPage.PageTitle / CartPage.PageTitle.
    /// </summary>
    public class CheckoutPage
    {
        private readonly IPage _page;

        public CheckoutPage(IPage page)
        {
            _page = page;
        }

        public string StepOneUrlPattern => ".*checkout-step-one.html";
        public string StepTwoUrlPattern => ".*checkout-step-two.html";
        public string CompleteUrlPattern => ".*checkout-complete.html";

        public ILocator PageTitle => _page.Locator("[data-test='title']");
        public ILocator ErrorMessage => _page.Locator("[data-test='error']");

        // Step One: customer info.
        public ILocator FirstNameInput => _page.GetByPlaceholder("First Name");
        public ILocator LastNameInput => _page.GetByPlaceholder("Last Name");
        public ILocator ZipCodeInput => _page.GetByPlaceholder("Zip/Postal Code");
        public ILocator ContinueButton => _page.GetByRole(AriaRole.Button, new() { Name = "Continue" });

        // Step Two: order overview.
        public ILocator OverviewItemNames => _page.Locator(".cart_item .inventory_item_name");
        public ILocator FinishButton => _page.GetByRole(AriaRole.Button, new() { Name = "Finish" });

        // Complete: confirmation.
        public ILocator OrderCompleteHeading => _page.GetByRole(AriaRole.Heading, new() { Name = "Thank you for your order!" });

        /// <summary>
        /// Fills the customer-info fields on Step One. Any parameter may be an empty string to
        /// exercise required-field validation.
        /// </summary>
        public async Task FillCustomerInfoAsync(string firstName, string lastName, string zipCode)
        {
            await FirstNameInput.FillAsync(firstName);
            await LastNameInput.FillAsync(lastName);
            await ZipCodeInput.FillAsync(zipCode);
        }

        /// <summary>
        /// Submits Step One's customer-info form.
        /// </summary>
        public async Task ContinueAsync()
        {
            await ContinueButton.ClickAsync();
        }

        /// <summary>
        /// Returns the visible product names listed on the Step Two order overview, in display order.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetOverviewItemNamesAsync()
        {
            return await OverviewItemNames.AllTextContentsAsync();
        }

        /// <summary>
        /// Completes the order from the Step Two overview.
        /// </summary>
        public async Task FinishAsync()
        {
            await FinishButton.ClickAsync();
        }
    }
}
