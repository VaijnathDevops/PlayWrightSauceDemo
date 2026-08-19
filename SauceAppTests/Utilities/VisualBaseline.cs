using Microsoft.Playwright;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SauceAppTests.Utilities
{
    /// <summary>
    /// Custom visual-regression baseline comparison, used by TC-NFR-001.
    ///
    /// Microsoft.Playwright.NUnit 1.52.0 does NOT expose a screenshot-comparison assertion:
    /// confirmed via reflection against the installed Microsoft.Playwright.dll that
    /// IPageAssertions only has ToHaveTitleAsync/ToHaveURLAsync, and ILocatorAssertions has no
    /// ToHaveScreenshotAsync member at all (both were enumerated in full - see the member list
    /// captured during automation; the JS/TS Playwright binding's page.toHaveScreenshot() was not
    /// ported to this version of the .NET binding). This class instead implements an equivalent
    /// pixel-diff comparison using SixLabors.ImageSharp (already a project dependency) against a
    /// baseline PNG committed under SauceAppTests/Baselines/.
    /// </summary>
    public static class VisualBaseline
    {
        // Test host runs from SauceAppTests/bin/Debug/net10.0 - three levels up is SauceAppTests/.
        private static string BaselineDirectory =>
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Baselines");

        private const double DefaultMaxDiffRatio = 0.01; // 1% of pixels - absorbs minor font/AA rendering jitter.
        private const int ChannelTolerance = 10; // per-channel RGB tolerance before a pixel counts as "different".

        /// <summary>
        /// Captures a full-page screenshot of <paramref name="page"/> and compares it against the
        /// committed baseline PNG named <paramref name="baselineFileName"/> under SauceAppTests/Baselines/.
        /// If no baseline exists yet, this call establishes one from the current screenshot (the
        /// comparison below still runs, trivially, against the file just written) and logs a note
        /// via TestContext - commit the resulting file and rerun to get a real comparison.
        /// </summary>
        public static async Task AssertMatchesBaselineAsync(
            IPage page,
            string baselineFileName,
            double maxDiffRatio = DefaultMaxDiffRatio)
        {
            var baselinePath = Path.Combine(BaselineDirectory, baselineFileName);

            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = true,
                Animations = ScreenshotAnimations.Disabled
            });

            if (!File.Exists(baselinePath))
            {
                Directory.CreateDirectory(BaselineDirectory);
                await File.WriteAllBytesAsync(baselinePath, screenshotBytes);
                TestContext.WriteLine(
                    $"No baseline found at '{baselinePath}'. Established a new baseline from this run's " +
                    "screenshot. Commit this file to source control; subsequent runs will compare against it.");
            }

            using var actual = Image.Load<Rgba32>(screenshotBytes);
            using var baseline = Image.Load<Rgba32>(baselinePath);

            Assert.That(actual.Width, Is.EqualTo(baseline.Width),
                $"Screenshot width ({actual.Width}px) differs from baseline width ({baseline.Width}px) - likely a layout change.");
            Assert.That(actual.Height, Is.EqualTo(baseline.Height),
                $"Screenshot height ({actual.Height}px) differs from baseline height ({baseline.Height}px) - likely a layout change.");

            long diffPixels = 0;
            long totalPixels = (long)actual.Width * actual.Height;

            for (var y = 0; y < actual.Height; y++)
            {
                for (var x = 0; x < actual.Width; x++)
                {
                    var a = actual[x, y];
                    var b = baseline[x, y];
                    if (Math.Abs(a.R - b.R) > ChannelTolerance ||
                        Math.Abs(a.G - b.G) > ChannelTolerance ||
                        Math.Abs(a.B - b.B) > ChannelTolerance)
                    {
                        diffPixels++;
                    }
                }
            }

            var diffRatio = totalPixels == 0 ? 0 : (double)diffPixels / totalPixels;
            Assert.That(diffRatio, Is.LessThanOrEqualTo(maxDiffRatio),
                $"Screenshot differs from baseline '{baselinePath}' by {diffRatio:P4} of pixels " +
                $"({diffPixels}/{totalPixels}), exceeding the {maxDiffRatio:P4} tolerance.");
        }
    }
}
