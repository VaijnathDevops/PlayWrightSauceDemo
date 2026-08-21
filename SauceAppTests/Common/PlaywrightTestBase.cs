using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using SauceAppTests.Pages;

namespace SauceAppTests.Common
{
    /// <summary>
    /// Shared base class for all Playwright/NUnit test fixtures in this project. Centralizes the
    /// screenshot-on-failure behavior so every fixture (Authentication, Checkout, NonFunctional,
    /// etc.) gets it automatically instead of duplicating the same [TearDown] method. The
    /// screenshot is both saved under the test's NUnit work directory and registered via
    /// TestContext.AddTestAttachment so it's linked from the generated HTML/TRX test report.
    /// </summary>
    [Parallelizable(ParallelScope.Self)]
    public abstract class PlaywrightTestBase : PageTest
    {
        /// <summary>
        /// Submits a valid standard-user login and waits for the redirect to the inventory page.
        /// Centralizes the "log in successfully" sequence repeated across fixtures (Authentication,
        /// Checkout, NonFunctional) instead of duplicating LoginAsync + the post-login URL assertion
        /// in every test. Callers are responsible for navigating to the login page first (via
        /// <paramref name="loginPage"/>.GotoAsync()) — this only covers the actual login action, so it
        /// isn't accidentally reused for negative-login-path tests.
        /// </summary>
        protected async Task LoginAsStandardUserAsync(LoginPage loginPage, InventoryPage inventoryPage)
        {
            await loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);
            await Expect(Page).ToHaveURLAsync(new Regex(inventoryPage.UrlPattern));
        }

        [TearDown]
        public async Task CaptureScreenshotOnFailureAsync()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                return;
            }

            var fileName = $"{TestContext.CurrentContext.Test.Name}-failure.png";
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);

            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });

            TestContext.AddTestAttachment(path, "Failure screenshot");
            TestContext.WriteLine($"Failure screenshot saved to {path}");
        }
    }
}
