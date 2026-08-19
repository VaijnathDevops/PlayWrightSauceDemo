using Microsoft.Playwright;

namespace SauceAppTests.Pages
{
    /// <summary>
    /// Page Object for the SauceDemo login page (https://www.saucedemo.com).
    /// Selectors verified against the live app's rendered DOM on 2026-08-18.
    ///
    /// Locator tier notes (see CLAUDE.md selector priority order):
    /// - Username/Password inputs have no associated &lt;label for="..."&gt; in the DOM, so GetByLabel
    ///   is not usable; they do have real placeholder text (placeholder="Username"/"Password"), so
    ///   GetByPlaceholder is the correct highest-available tier. Confirmed via live inspection - each
    ///   resolves to exactly one element and a real login succeeds through them.
    /// - Login is &lt;input type="submit" value="Login"&gt;, which has an implicit accessible "button"
    ///   role with its name computed from the value attribute, so GetByRole(Button, "Login") applies
    ///   and was confirmed to resolve uniquely.
    /// - The error banner is &lt;h3 data-test="error"&gt;...&lt;/h3&gt; with no role="alert"/aria-live and no
    ///   fixed text (message content varies per scenario under test), so GetByRole(Alert) and GetByText
    ///   are not viable for a locator meant to find the banner generically. GetByTestId is also not
    ///   usable here: it maps to Playwright's default data-testid attribute, not SauceDemo's data-test
    ///   attribute, and this project has not called Selectors.SetTestIdAttribute("data-test") anywhere
    ///   (confirmed - no such call exists outside the vendored Playwright driver under bin/). The raw
    ///   CSS attribute selector is therefore the documented last-resort choice for this element only.
    /// </summary>
    public class LoginPage
    {
        private readonly IPage _page;

        public LoginPage(IPage page)
        {
            _page = page;
        }

        public string Url => "https://www.saucedemo.com";

        public ILocator UsernameInput => _page.GetByPlaceholder("Username");
        public ILocator PasswordInput => _page.GetByPlaceholder("Password");
        public ILocator LoginButton => _page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        public ILocator ErrorMessage => _page.Locator("[data-test='error']");

        public async Task GotoAsync()
        {
            await _page.GotoAsync(Url);
        }

        /// <summary>
        /// Fills username and password (either may be empty string) and clicks Login.
        /// Credentials are always supplied by the caller — never hardcoded here.
        /// </summary>
        public async Task LoginAsync(string username, string password)
        {
            await UsernameInput.FillAsync(username);
            await PasswordInput.FillAsync(password);
            await LoginButton.ClickAsync();
        }
    }
}
