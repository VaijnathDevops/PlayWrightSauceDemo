using Microsoft.Playwright;

namespace TestProject1.Pages
{
    /// <summary>
    /// Page Object for the SauceDemo post-login inventory/products page
    /// (https://www.saucedemo.com/inventory.html).
    /// Selectors verified against the live app on 2026-08-18.
    ///
    /// Logout control notes (see CLAUDE.md selector priority order):
    /// - The hamburger menu is &lt;button id="react-burger-menu-btn"&gt;Open Menu&lt;/button&gt; — its visible
    ///   text content is "Open Menu" even though it's rendered off-screen/zero font-size, so it has a real
    ///   accessible "button" role with name "Open Menu"; GetByRole(Button, "Open Menu") resolves uniquely.
    ///   Confirmed live: clicking it reveals the sidebar nav (#logout_sidebar_link etc.) become visible.
    /// - Logout is &lt;a id="logout_sidebar_link" data-test="logout-sidebar-link"&gt;Logout&lt;/a&gt; inside that
    ///   sidebar nav — an anchor with visible text "Logout" has an implicit "link" accessible role, so
    ///   GetByRole(Link, "Logout") resolves uniquely once the menu is open. Confirmed live: clicking it
    ///   redirects to https://www.saucedemo.com/ and shows the login form again.
    ///
    /// Sort/cart control notes (selectors verified against the live app on 2026-08-18):
    /// - The product sort control is &lt;select class="product_sort_container" data-test="product-sort-container"&gt;
    ///   with no associated &lt;label&gt;/aria-label, but a &lt;select&gt; has an implicit "combobox" accessible role
    ///   regardless, and it's the only combobox on the page — confirmed live (count == 1) — so
    ///   GetByRole(Combobox) resolves uniquely without needing a Name filter. Options are plain text
    ///   ("Name (A to Z)", "Name (Z to A)", "Price (low to high)", "Price (high to low)"), selected via
    ///   SelectOptionAsync(new SelectOptionValue { Label = ... }).
    /// - Each product card is &lt;div class="inventory_item"&gt; containing an &lt;.inventory_item_name&gt; with the
    ///   product's display name and an "Add to cart"/"Remove" &lt;button&gt; (plain visible text, implicit
    ///   "button" role). Since every card's button shares the same text, a single global
    ///   GetByRole(Button, "Add to cart") would match multiple elements — the correct role-based pattern
    ///   (confirmed live) is to first scope to the specific card via
    ///   Locator(".inventory_item").Filter(new() { HasText = productName }), then GetByRole within that
    ///   scoped locator, which resolves to exactly one button per product.
    /// - The cart link is &lt;a class="shopping_cart_link" data-test="shopping-cart-link"&gt;&lt;/a&gt; — it has no
    ///   href attribute and no text/aria-label content, so it is not exposed with any accessible
    ///   role/name at all (confirmed via AriaSnapshotAsync: it does not appear in the accessibility tree).
    ///   GetByRole/GetByText/GetByLabel are therefore not viable here; the raw CSS class selector is the
    ///   documented last resort for this element only, consistent with the existing ErrorMessage pattern
    ///   in LoginPage.
    /// </summary>
    public class InventoryPage
    {
        private readonly IPage _page;

        public InventoryPage(IPage page)
        {
            _page = page;
        }

        public string UrlPattern => ".*inventory.html";

        public ILocator PageTitle => _page.Locator("[data-test='title']");
        public ILocator InventoryContainer => _page.Locator("[data-test='inventory-container']");
        public ILocator BurgerMenuButton => _page.GetByRole(AriaRole.Button, new() { Name = "Open Menu" });
        public ILocator LogoutLink => _page.GetByRole(AriaRole.Link, new() { Name = "Logout" });

        public ILocator SortDropdown => _page.GetByRole(AriaRole.Combobox);
        public ILocator CartLink => _page.Locator(".shopping_cart_link");
        public ILocator CartBadge => _page.Locator(".shopping_cart_badge");

        /// <summary>
        /// Scopes to the single product card whose visible text contains <paramref name="productName"/>.
        /// </summary>
        public ILocator ProductCard(string productName) =>
            _page.Locator(".inventory_item").Filter(new() { HasText = productName });

        /// <summary>
        /// Opens the hamburger menu and clicks Logout, returning the user to the login page.
        /// </summary>
        public async Task LogoutAsync()
        {
            await BurgerMenuButton.ClickAsync();
            await LogoutLink.ClickAsync();
        }

        /// <summary>
        /// Selects a sort option from the product sort dropdown by its visible option label
        /// (e.g. "Name (Z to A)", "Price (low to high)").
        /// </summary>
        public async Task SortByAsync(string optionLabel)
        {
            await SortDropdown.SelectOptionAsync(new SelectOptionValue { Label = optionLabel });
        }

        /// <summary>
        /// Clicks "Add to cart" on the product card matching the given product name.
        /// </summary>
        public async Task AddProductToCartAsync(string productName)
        {
            await ProductCard(productName).GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();
        }

        /// <summary>
        /// Navigates to the cart page via the cart link in the header.
        /// </summary>
        public async Task GoToCartAsync()
        {
            await CartLink.ClickAsync();
        }
    }
}
