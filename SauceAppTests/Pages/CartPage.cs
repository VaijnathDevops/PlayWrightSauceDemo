using Microsoft.Playwright;

namespace SauceAppTests.Pages
{
    /// <summary>
    /// Page Object for the SauceDemo cart page (https://www.saucedemo.com/cart.html).
    /// Selectors verified against the live app on 2026-08-18.
    ///
    /// Locator tier notes (see CLAUDE.md selector priority order):
    /// - Each cart line is &lt;div class="cart_item"&gt; containing an &lt;.inventory_item_name&gt; with the
    ///   product's display name — confirmed live via outerHTML inspection.
    /// - "Checkout" is &lt;button data-test="checkout" id="checkout"&gt;Checkout&lt;/button&gt; — plain visible
    ///   text with an implicit "button" accessible role, confirmed unique on the page (count == 1), so
    ///   GetByRole(Button, "Checkout") is used rather than the data-test/id attribute.
    /// - "Continue Shopping" is &lt;button data-test="continue-shopping" id="continue-shopping"&gt;Continue
    ///   Shopping&lt;/button&gt; — despite the "back" styling class it is a real &lt;button&gt; (not an anchor),
    ///   confirmed live via outerHTML inspection, with a unique accessible "button" role/name, and
    ///   confirmed to navigate to the inventory page when clicked.
    /// - The page's sub-header ("Your Cart") uses the same &lt;[data-test='title']&gt; element already used
    ///   as InventoryPage.PageTitle for "Products" — reusing that established selector for consistency
    ///   rather than introducing a new pattern for what is the same element type across pages.
    /// </summary>
    public class CartPage
    {
        private readonly IPage _page;

        public CartPage(IPage page)
        {
            _page = page;
        }

        public string UrlPattern => ".*cart.html";

        public ILocator PageTitle => _page.Locator("[data-test='title']");
        public ILocator CartItems => _page.Locator(".cart_item");
        public ILocator CartItemNames => _page.Locator(".cart_item .inventory_item_name");
        public ILocator CheckoutButton => _page.GetByRole(AriaRole.Button, new() { Name = "Checkout" });
        public ILocator ContinueShoppingButton => _page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });

        /// <summary>
        /// Returns the visible product names currently listed in the cart, in display order.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetCartItemNamesAsync()
        {
            return await CartItemNames.AllTextContentsAsync();
        }

        /// <summary>
        /// Clicks Checkout to begin the checkout flow (customer-info step).
        /// </summary>
        public async Task ProceedToCheckoutAsync()
        {
            await CheckoutButton.ClickAsync();
        }

        /// <summary>
        /// Clicks Continue Shopping to return to the inventory page.
        /// </summary>
        public async Task ContinueShoppingAsync()
        {
            await ContinueShoppingButton.ClickAsync();
        }
    }
}
