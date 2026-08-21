using Microsoft.Playwright;
using SauceAppTests.Common;
using SauceAppTests.DTOs;
using SauceAppTests.Pages;
using SauceAppTests.Utilities;

namespace SauceAppTests
{
    /// <summary>
    /// Automates TestCases/Checkout.md (TC-CHECKOUT-001 through 003) against the live SauceDemo app
    /// (https://www.saucedemo.com). Selectors, sort options, and checkout copy verified against the
    /// live app on 2026-08-18. Expected error copy is sourced from the shared
    /// <see cref="ErrorMessages.Checkout"/> class rather than duplicated here.
    /// </summary>
    [TestFixture]
    public class CheckoutTests : Common.PlaywrightTestBase
    {
        private const string OrderConfirmationHeading = "Thank you for your order!";

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
            await LoginAsStandardUserAsync(_loginPage, _inventoryPage);

            // Test host runs from SauceAppTests/bin/Debug/net10.0 - three levels up is SauceAppTests/.
            var testDataDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

            var products = CsvExposureHelper.ReadCsvToObject<ProductDTO>(Path.Combine(testDataDirectory, "products.csv"));
            var firstProduct = products.First(p => p.Key == "First");
            var secondProduct = products.First(p => p.Key == "Second");
            _firstProduct = firstProduct.Name;
            _secondProduct = secondProduct.Name;

            var customers = CsvExposureHelper.ReadCsvToObject<CustomerDTO>(Path.Combine(testDataDirectory, "customers.csv"));
            var customer = customers.First(c => c.Key == "Default");
            _customerFirstName = customer.FirstName;
            _customerLastName = customer.LastName;
            _customerZipCode = customer.ZipCode;
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
        [Description("TC-CHECKOUT-002: Checkout must not proceed with an empty cart. KNOWN BUG (see TestCases/Checkout.md, BUG-CHECKOUT-001): the live SauceDemo app does not enforce this - clicking Checkout with zero items proceeds straight to the customer-info step anyway. This test encodes the correct expected behavior and is expected to fail until the app is fixed; per this repo's self-healing policy it is reported as a product bug candidate rather than weakened to force a green result.")]
        public async Task Checkout_WithEmptyCart_DoesNotProceedToCustomerInfoStep()
        {
            await Expect(_inventoryPage.CartBadge).Not.ToBeVisibleAsync();

            // 1 & 2. Open the cart without adding any items; the cart is shown as empty.
            await _inventoryPage.GoToCartAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_cartPage.UrlPattern));
            await Expect(_cartPage.CartItems).ToHaveCountAsync(0);

            // 3. Attempt to proceed to checkout with no product in the cart.
            await _cartPage.ProceedToCheckoutAsync();

            // Expected (correct) behavior: checkout is blocked and the user stays on the cart page -
            // the customer-info step must never be reached with an empty cart.
            await Expect(Page).ToHaveURLAsync(new Regex(_cartPage.UrlPattern));
            await Expect(Page).Not.ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
        }

        [Test]
        [Description("TC-CHECKOUT-003: Checkout customer-info form validates First Name, Last Name, and Zip/Postal Code as required, each with its own error message, and only proceeds when all three are provided")]
        [TestCase(CheckoutValidationScenario.MissingFirstName)]
        [TestCase(CheckoutValidationScenario.MissingLastName)]
        [TestCase(CheckoutValidationScenario.MissingPostalCode)]
        [TestCase(CheckoutValidationScenario.AllFieldsMissing)]
        [TestCase(CheckoutValidationScenario.AllFieldsValid)]
        public async Task Checkout_WithVariousFieldCombinations_ProducesExpectedOutcome(CheckoutValidationScenario scenario)
        {
            await _inventoryPage.AddProductToCartAsync(_firstProduct);
            await _inventoryPage.GoToCartAsync();
            await _cartPage.ProceedToCheckoutAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));

            var (firstName, lastName, zipCode) = scenario switch
            {
                CheckoutValidationScenario.MissingFirstName => (string.Empty, _customerLastName, _customerZipCode),
                CheckoutValidationScenario.MissingLastName => (_customerFirstName, string.Empty, _customerZipCode),
                CheckoutValidationScenario.MissingPostalCode => (_customerFirstName, _customerLastName, string.Empty),
                CheckoutValidationScenario.AllFieldsMissing => (string.Empty, string.Empty, string.Empty),
                CheckoutValidationScenario.AllFieldsValid => (_customerFirstName, _customerLastName, _customerZipCode),
                _ => throw new ArgumentOutOfRangeException(nameof(scenario))
            };

            await _checkoutPage.FillCustomerInfoAsync(firstName, lastName, zipCode);
            await _checkoutPage.ContinueAsync();

            switch (scenario)
            {
                case CheckoutValidationScenario.MissingFirstName:
                    await Expect(_checkoutPage.ErrorMessage).ToHaveTextAsync(ErrorMessages.Checkout.FirstNameRequired);
                    await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
                    break;
                case CheckoutValidationScenario.MissingLastName:
                    await Expect(_checkoutPage.ErrorMessage).ToHaveTextAsync(ErrorMessages.Checkout.LastNameRequired);
                    await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
                    break;
                case CheckoutValidationScenario.MissingPostalCode:
                    await Expect(_checkoutPage.ErrorMessage).ToHaveTextAsync(ErrorMessages.Checkout.PostalCodeRequired);
                    await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
                    break;
                case CheckoutValidationScenario.AllFieldsMissing:
                    // Confirmed live (2026-08-21): the app validates fields in DOM order (First Name,
                    // then Last Name, then Zip/Postal Code) and reports only the first blank one it
                    // finds, so with all three empty it shows the same error as MissingFirstName.
                    await Expect(_checkoutPage.ErrorMessage).ToHaveTextAsync(ErrorMessages.Checkout.FirstNameRequired);
                    await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));
                    break;
                case CheckoutValidationScenario.AllFieldsValid:
                    await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepTwoUrlPattern));
                    await Expect(_checkoutPage.PageTitle).ToHaveTextAsync("Checkout: Overview");
                    break;
            }
        }

        [Test]
        [Description("TC-CHECKOUT-004: 'Continue Shopping' on the cart page returns the user to the inventory page")]
        public async Task ContinueShopping_FromCartPage_ReturnsToInventoryPage()
        {
            await _inventoryPage.AddProductToCartAsync(_firstProduct);
            await _inventoryPage.GoToCartAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_cartPage.UrlPattern));

            await _cartPage.ContinueShoppingAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(_inventoryPage.UrlPattern));
            await Expect(_inventoryPage.PageTitle).ToHaveTextAsync("Products");
        }

        [Test]
        [Description("TC-CHECKOUT-005: 'Cancel' on the customer-info step returns the user to the cart page")]
        public async Task Cancel_FromCustomerInfoStep_ReturnsToCartPage()
        {
            await _inventoryPage.AddProductToCartAsync(_firstProduct);
            await _inventoryPage.GoToCartAsync();
            await _cartPage.ProceedToCheckoutAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(_checkoutPage.StepOneUrlPattern));

            await _checkoutPage.CancelAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(_cartPage.UrlPattern));
            await Expect(_cartPage.CartItemNames).ToHaveTextAsync(new[] { _firstProduct });
        }
    }
}
