using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using SauceAppTests.Pages;
using SauceAppTests.Utilities;

namespace SauceAppTests
{
    /// <summary>
    /// Automates TestCases/NonFunctional.md (TC-NFR-001 through TC-NFR-004) - cross-cutting
    /// visual regression, network/API validation, network mocking, and accessibility techniques
    /// applied to the live SauceDemo app (https://www.saucedemo.com).
    ///
    /// Live investigation performed during automation (2026-08-18), which resolved every [TBD] in
    /// the manual test case file:
    /// - SauceDemo is a static SPA with no conventional backend REST API. The only runtime network
    ///   calls beyond static assets/images are: (a) a Google Fonts stylesheet request to
    ///   https://fonts.googleapis.com/css2?... fired on every page load, which returns a real 200
    ///   text/css response - used for TC-NFR-002 and TC-NFR-003 below; and (b) telemetry beacons to
    ///   events.backtrace.io that consistently return 401 (a placeholder/demo token, not something
    ///   worth asserting a 2xx contract against). Login/checkout submission are purely client-side -
    ///   no request fires when the login form or checkout form is submitted, so there is nothing to
    ///   intercept there for TC-NFR-003; the Google Fonts request was used instead, per the
    ///   documented fallback in the manual test case file.
    /// - Microsoft.Playwright.NUnit 1.52.0 has no ToHaveScreenshotAsync (see
    ///   SauceAppTests.Utilities.VisualBaseline for the confirmed reflection findings and the
    ///   pixel-diff-based replacement used for TC-NFR-001).
    /// - The .NET-compatible axe-core binding used for TC-NFR-004 is Deque.AxeCore.Playwright
    ///   4.13.0 (NuGet, already referenced in SauceAppTests.csproj), which wraps the real axe-core
    ///   engine (confirmed: AxeResult.TestEngine reports "axe-core 4.13.0" at scan time).
    /// </summary>
    [TestFixture]
    public class NonFunctionalTests : Common.PlaywrightTestBase
    {
        // Pins the viewport so TC-NFR-001's screenshot comparison is deterministic regardless of
        // headed/headless mode or the host window's size (the original baseline was captured without
        // this pin and broke on a headless rerun at the runtime's default 1280px-wide viewport).
        public override BrowserNewContextOptions ContextOptions() => new()
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        };

        // ---------------------------------------------------------------------------------------
        // TC-NFR-001: Visual regression
        // ---------------------------------------------------------------------------------------

        [Test]
        [Description("TC-NFR-001: Inventory page has no unexpected visual differences from baseline")]
        public async Task InventoryPage_VisualAppearance_MatchesBaseline()
        {
            var loginPage = new LoginPage(Page);
            var inventoryPage = new InventoryPage(Page);

            await loginPage.GotoAsync();
            await loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);

            await Expect(Page).ToHaveURLAsync(new Regex(inventoryPage.UrlPattern));
            await Expect(inventoryPage.InventoryContainer).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await VisualBaseline.AssertMatchesBaselineAsync(Page, "inventory-page.png");
        }

        // ---------------------------------------------------------------------------------------
        // TC-NFR-002: Key network request during core workflow returns a valid, well-formed response
        // ---------------------------------------------------------------------------------------

        [Test]
        [Description("TC-NFR-002: Google Fonts stylesheet request fired on page load returns a valid, well-formed CSS response")]
        public async Task GoogleFontsStylesheetRequest_OnPageLoad_ReturnsValidCssResponse()
        {
            var loginPage = new LoginPage(Page);

            var responseTask = Page.WaitForResponseAsync(new Regex(@"^https://fonts\.googleapis\.com/css2\?"));
            await loginPage.GotoAsync();
            var response = await responseTask;

            Assert.That(response.Ok, Is.True, $"Expected a successful response; got status {response.Status}.");
            Assert.That(response.Status, Is.EqualTo(200));

            var contentType = await response.HeaderValueAsync("content-type");
            Assert.That(contentType, Does.Contain("text/css"));

            var body = await response.TextAsync();
            Assert.That(body, Does.Contain("@font-face"),
                "Expected the Google Fonts stylesheet body to contain @font-face rules.");

            // The corresponding UI state reflects the response correctly: the page renders normally.
            await Expect(loginPage.LoginButton).ToBeVisibleAsync();
        }

        // ---------------------------------------------------------------------------------------
        // TC-NFR-003: UI handles a mocked/stubbed error response correctly
        // ---------------------------------------------------------------------------------------

        [Test]
        [Description("TC-NFR-003: App remains functional (renders and allows login) when the Google Fonts stylesheet request is mocked to fail")]
        public async Task LoginAndInventory_WithMockedGoogleFontsFailure_StillRenderAndFunction()
        {
            var loginPage = new LoginPage(Page);
            var inventoryPage = new InventoryPage(Page);

            await Page.RouteAsync("https://fonts.googleapis.com/**", async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions
                {
                    Status = 500,
                    ContentType = "text/plain",
                    Body = "mocked failure"
                });
            });

            await loginPage.GotoAsync();
            await Expect(loginPage.LoginButton).ToBeVisibleAsync();

            await loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);

            await Expect(Page).ToHaveURLAsync(new Regex(inventoryPage.UrlPattern));
            await Expect(inventoryPage.PageTitle).ToHaveTextAsync("Products");
        }

        // ---------------------------------------------------------------------------------------
        // TC-NFR-004: Automated accessibility scan finds no critical/serious violations on a key page
        // ---------------------------------------------------------------------------------------

        [Test]
        [Description("TC-NFR-004: Accessibility scan of the login page (logged out) finds no critical/serious violations")]
        public async Task LoginPage_AccessibilityScan_HasNoCriticalOrSeriousViolations()
        {
            var loginPage = new LoginPage(Page);
            await loginPage.GotoAsync();
            await Expect(loginPage.LoginButton).ToBeVisibleAsync();

            var result = await Page.RunAxe(new AxeRunOptions());
            var blocking = AccessibilityAssertions.GetCriticalOrSeriousViolations(result);

            Assert.That(blocking, Is.Empty,
                "Expected no critical/serious accessibility violations on the login page. Found:" +
                Environment.NewLine + AccessibilityAssertions.Describe(blocking));
        }

        [Test]
        [Description("TC-NFR-004: Accessibility scan of the inventory page (post-login) finds no critical/serious violations")]
        public async Task InventoryPage_AccessibilityScan_HasNoCriticalOrSeriousViolations()
        {
            var loginPage = new LoginPage(Page);
            var inventoryPage = new InventoryPage(Page);

            await loginPage.GotoAsync();
            await loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);
            await Expect(Page).ToHaveURLAsync(new Regex(inventoryPage.UrlPattern));
            await Expect(inventoryPage.InventoryContainer).ToBeVisibleAsync();

            var result = await Page.RunAxe(new AxeRunOptions());
            var blocking = AccessibilityAssertions.GetCriticalOrSeriousViolations(result);

            // NOTE (confirmed live, 2026-08-18): this assertion currently fails due to a genuine
            // SauceDemo defect, not a test/selector problem - the product-sort <select> element has
            // no accessible name (axe rule "select-name", impact "critical"). This is a real product
            // accessibility bug candidate; see TestCases/NonFunctional.md for the full finding. Per
            // this repo's self-healing policy, that finding is reported rather than the assertion
            // being weakened to force a green result.
            Assert.That(blocking, Is.Empty,
                "Expected no critical/serious accessibility violations on the inventory page. Found:" +
                Environment.NewLine + AccessibilityAssertions.Describe(blocking));
        }
    }
}
