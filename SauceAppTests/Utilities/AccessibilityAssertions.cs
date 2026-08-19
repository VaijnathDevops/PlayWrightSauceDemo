using Deque.AxeCore.Commons;

namespace SauceAppTests.Utilities
{
    /// <summary>
    /// Helpers around Deque.AxeCore.Playwright (the .NET-compatible axe-core binding used in place of
    /// the JS-only @axe-core/playwright package named in the original requirement) for TC-NFR-004.
    /// Deque.AxeCore.Commons.AxeResultItem.Impact uses the same severity vocabulary as axe-core's own
    /// JS API: "minor", "moderate", "serious", "critical" (confirmed via reflection against the
    /// installed Deque.AxeCore.Commons assembly and the actual string values returned by a live scan).
    /// </summary>
    public static class AccessibilityAssertions
    {
        private static readonly string[] BlockingSeverities = { "critical", "serious" };

        /// <summary>
        /// Returns only the violations from <paramref name="result"/> whose impact is "critical" or
        /// "serious" - the severities TC-NFR-004 requires to be zero.
        /// </summary>
        public static AxeResultItem[] GetCriticalOrSeriousViolations(AxeResult result) =>
            result.Violations.Where(v => BlockingSeverities.Contains(v.Impact)).ToArray();

        /// <summary>
        /// Renders violations as a human-readable multi-line string for assertion failure messages.
        /// </summary>
        public static string Describe(IEnumerable<AxeResultItem> violations) =>
            string.Join(Environment.NewLine, violations.Select(v =>
                $"[{v.Impact}] {v.Id}: {v.Help} ({v.Nodes.Length} node(s)) - {v.HelpUrl}"));
    }
}
