using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace TestProject1.Common
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
