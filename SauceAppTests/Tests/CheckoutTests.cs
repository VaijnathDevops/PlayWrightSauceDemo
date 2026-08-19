using Microsoft.Playwright;
using SauceAppTests.Pages;
using SauceAppTests.Utilities;

namespace SauceAppTests
{
    /// <summary>
    /// Automates TestCases/Checkout.md (TC-CHECKOUT-001 through 003) against the live SauceDemo app
    /// (https://www.saucedemo.com). Selectors, sort options, checkout copy, and the empty-cart
    /// checkout behavior verified against the live app on 2026-08-18.
    /// </summary>
    [TestFixture]
    public class CheckoutTests : Common.PlaywrightTestBase
    {
        private const string OrderConfirmationHeading = "Thank you for your order!";
        private const string PostalCodeRequiredError = "Error: Postal Code is required";

        private LoginPage _loginPage = null!;
        private InventoryPage _inventoryPage = null!;
        private CartPage _cartPage = null!;
        private CheckoutPage _checkoutPage = null!;

        // Product and customer data are sourced from SauceAppTests/TestData/*.csv rather than
        // hardcoded here, so test data can be updated without touching test code.
        private string _firstProduct = null!;
        private string _secondProduct = null!;
        private string _customerFirstName = null!;
        private string _customerLastName = null!;
        private string _customerZipCode = null!;

        // Every test in this fixture starts the same way: logged in and on the inventory page —
        // genuinely uniform across all three cases, so it's hoisted here instead of repeated.
        [SetUp]
        public async Task SetUpAsync()
        {
            _loginPage = new LoginPage(Page);
            _inventoryPage = new InventoryPage(Page);
            _cartPage = new CartPage(Page);
            _checkoutPage = new CheckoutPage(Page);

            await _loginPage.GotoAsync();
            await _loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);
            await Expect(Page).ToHaveURLAsync(new Regex(_inventoryPage.UrlPattern));

            var firstProductRow = TestDataReader.GetRow("products.csv", "First");
            var secondProductRow = TestDataReader.GetRow("products.csv", "Second");
            _firstProduct = firstProductRow["Name"];
            _secondProduct = secondProductRow["Name"];

            var customerRow = TestDataReader.GetRow("customers.csv", "Default");
            _customerFirstName = customerRow["FirstName"];
            _customerLastName = customerRow["LastName"];
            _customerZipCode = customerRow["ZipCode"];
        }

        // Screenshot-on-failure is provided by the shared SauceAppTests.Common.PlaywrightTestBase
        // [TearDown], so it doesn't need to be duplicated here.

        [Test]
        [Description("TC-CHECKOUT-001: Full purchase journey - browse, sort, add to cart, and complete checkout")]
        public async Task Checkout_FullPurchaseJourney_CompletesOrderAndEmptiesCart()
        {
            // 1. Sort by a non-default option.
            await _inventoryPage.SortByAsync("Name (Z to A)");

            // 2 & 3. Add two distinct products to the cart.
            await _inventoryPage.AddProductToCartAsync(_firstProduct);
            await _inventoryPage.AddProductToCartAsync(_secondProduct);
            await Expect(_inventoryPage.CartBadge).ToHaveTextAsync("2");

            // 4 & 5. Open the cart and verify both items are listed.
            await _inventoryPage.GoToCartAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_cartPage.UrlPattern));
            await Expect(_cartPage.CartItemNames).ToHaveTextAsync(new[] { _firstProduct, _secondProduct });

            // 6. Click Checkout.
            await _cartPage.ProceedToCheckoutAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
            await Expect(_checkoutPage.PageTitle).ToHaveTextAsync("Checkout: Your Information");

            // 7. Enter customer info.
            await _checkoutPage.FillCustomerInfoAsync(_customerFirstName, _customerLastName, _customerZipCode);

            // 8. Proceed to the order overview step.
            await _checkoutPage.ContinueAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepTwoUrlPattern));
            await Expect(_checkoutPage.PageTitle).ToHaveTextAsync("Checkout: Overview");
            await Expect(_checkoutPage.OverviewItemNames).ToHaveTextAsync(new[] { _firstProduct, _secondProduct });

            // 9. Complete the order.
            await _checkoutPage.FinishAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.CompleteUrlPattern));
            await Expect(_checkoutPage.OrderCompleteHeading).ToBeVisibleAsync();
            await Expect(_checkoutPage.PageTitle).ToHaveTextAsync("Checkout: Complete!");

            // The cart is empty afterward.
            await Expect(_inventoryPage.CartBadge).Not.ToBeVisibleAsync();
            await Page.GotoAsync("https://www.saucedemo.com/cart.html");
            await Expect(_cartPage.CartItems).ToHaveCountAsync(0);
        }

        [Test]
        [Description("TC-CHECKOUT-002: Checkout with an empty cart - the cart shows no items, and SauceDemo's Checkout control (confirmed live) does not block proceeding to the customer-info step despite the empty cart")]
        public async Task Checkout_WithEmptyCart_ShowsEmptyCartAndDoesNotBlockCheckout()
        {
            await Expect(_inventoryPage.CartBadge).Not.ToBeVisibleAsync();

            // 1 & 2. Open the cart without adding any items; the cart is shown as empty.
            await _inventoryPage.GoToCartAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_cartPage.UrlPattern));
            await Expect(_cartPage.CartItems).ToHaveCountAsync(0);

            // 3. Attempt to proceed to checkout: the Checkout control is available and enabled, and
            // clicking it does not block the user - it proceeds straight to the customer-info step,
            // exactly as it would with items in the cart. This is confirmed live behavior of the public
            // SauceDemo app (it does not enforce a non-empty cart before allowing checkout to begin).
            await Expect(_cartPage.CheckoutButton).ToBeVisibleAsync();
            await Expect(_cartPage.CheckoutButton).ToBeEnabledAsync();
            await _cartPage.ProceedToCheckoutAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
        }

        [Test]
        [Description("TC-CHECKOUT-003: Checkout customer-info form shows a validation error when a required field (Zip/Postal Code) is missing")]
        public async Task Checkout_WithMissingPostalCode_ShowsValidationErrorAndDoesNotProceed()
        {
            await _inventoryPage.AddProductToCartAsync(_firstProduct);
            await _inventoryPage.GoToCartAsync();
            await _cartPage.ProceedToCheckoutAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));

            // 1. Leave Zip/Postal Code empty. 2. Click Continue.
            await _checkoutPage.FillCustomerInfoAsync(_customerFirstName, _customerLastName, string.Empty);
            await _checkoutPage.ContinueAsync();

            // Form submission is rejected; the user remains on the customer-info step with a
            // validation error, and does not proceed to the overview step.
            await Expect(_checkoutPage.ErrorMessage).ToHaveTextAsync(PostalCodeRequiredError);
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
            await Expect(_checkoutPage.PageTitle).ToHaveTextAsync("Checkout: Your Information");
        }
    }
}
