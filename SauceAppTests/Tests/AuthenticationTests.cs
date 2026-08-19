using Microsoft.Playwright;
using TestProject1.Common;
using TestProject1.Pages;

namespace TestProject1
{
    /// <summary>
    /// Automates TestCases/Login.md (TC-LOGIN-001 through 008) and TestCases/Logout.md
    /// (TC-LOGOUT-001/002) against the live SauceDemo app (https://www.saucedemo.com). Grouped into
    /// one Authentication fixture — Login and Logout share the same Page Objects and concern, so
    /// they don't need separate files (see CLAUDE.md's test-fixture grouping convention).
    /// Selectors and error copy verified against the live app on 2026-08-18.
    /// </summary>
    [TestFixture]
    public class AuthenticationTests : Common.PlaywrightTestBase
    {
        private const string InvalidCredentialsError = "Epic sadface: Username and password do not match any user in this service";
        private const string LockedOutError = "Epic sadface: Sorry, this user has been locked out.";
        private const string UsernameRequiredError = "Epic sadface: Username is required";
        private const string PasswordRequiredError = "Epic sadface: Password is required";
        private const string ProtectedPageError = "Epic sadface: You can only access '/inventory.html' when you are logged in.";
        private const string InventoryUrl = "https://www.saucedemo.com/inventory.html";

        // Deliberately fake values used only to exercise negative paths — never real credentials.
        private const string WrongPassword = "not-the-real-password";
        private const string NonExistentUsername = "not_a_real_user_12345";

        private LoginPage _loginPage = null!;
        private InventoryPage _inventoryPage = null!;

        // Every test in this fixture starts logged out on the login page — genuinely uniform, so
        // it's hoisted here instead of repeated in each test.
        [SetUp]
        public async Task SetUpAsync()
        {
            _loginPage = new LoginPage(Page);
            _inventoryPage = new InventoryPage(Page);
            await _loginPage.GotoAsync();
        }

        // Screenshot-on-failure is provided by the shared TestProject1.Common.PlaywrightTestBase
        // [TearDown], so it doesn't need to be duplicated here.

        // -----------------------------------------------------------------------------------------
        // Login (TestCases/Login.md)
        // -----------------------------------------------------------------------------------------

        [Test]
        [Description("TC-LOGIN-001: Valid login with standard user succeeds")]
        public async Task Login_WithStandardUser_RedirectsToInventoryPage()
        {
            await _loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);

            await Expect(Page).ToHaveURLAsync(new Regex(_inventoryPage.UrlPattern));
            await Expect(_inventoryPage.PageTitle).ToHaveTextAsync("Products");
            await Expect(_loginPage.LoginButton).Not.ToBeVisibleAsync();
        }

        [Test]
        [Description("TC-LOGIN-002: Login fails with incorrect password for a valid username")]
        public async Task Login_WithIncorrectPassword_ShowsInvalidCredentialsError()
        {
            await _loginPage.LoginAsync(TestSettings.StandardUsername, WrongPassword);

            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(InvalidCredentialsError);
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
        }

        [Test]
        [Description("TC-LOGIN-003: Login fails with unknown/nonexistent username")]
        public async Task Login_WithUnknownUsername_ShowsInvalidCredentialsError()
        {
            await _loginPage.LoginAsync(NonExistentUsername, TestSettings.Password);

            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(InvalidCredentialsError);
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
        }

        [Test]
        [Description("TC-LOGIN-004: Login fails for locked-out user account")]
        public async Task Login_WithLockedOutUser_ShowsLockedOutError()
        {
            await _loginPage.LoginAsync(TestSettings.LockedOutUsername, TestSettings.Password);

            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(LockedOutError);
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
        }

        [Test]
        [Description("TC-LOGIN-005: Login fails when username is empty")]
        public async Task Login_WithEmptyUsername_ShowsUsernameRequiredError()
        {
            await _loginPage.LoginAsync(string.Empty, TestSettings.Password);

            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(UsernameRequiredError);
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
        }

        [Test]
        [Description("TC-LOGIN-006: Login fails when password is empty")]
        public async Task Login_WithEmptyPassword_ShowsPasswordRequiredError()
        {
            await _loginPage.LoginAsync(TestSettings.StandardUsername, string.Empty);

            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(PasswordRequiredError);
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
        }

        [Test]
        [Description("TC-LOGIN-007: Login fails when both username and password are empty (username validation takes precedence)")]
        public async Task Login_WithEmptyUsernameAndPassword_ShowsUsernameRequiredError()
        {
            await _loginPage.LoginAsync(string.Empty, string.Empty);

            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(UsernameRequiredError);
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
        }

        [Test]
        [Description("TC-LOGIN-008: Login outcome varies correctly across multiple credential sets")]
        [TestCase(LoginCredentialSet.StandardUser)]
        [TestCase(LoginCredentialSet.LockedOutUser)]
        [TestCase(LoginCredentialSet.StandardUserWithWrongPassword)]
        public async Task Login_WithVariousCredentialSets_ProducesExpectedOutcome(LoginCredentialSet credentialSet)
        {
            var (username, password) = credentialSet switch
            {
                LoginCredentialSet.StandardUser => (TestSettings.StandardUsername, TestSettings.Password),
                LoginCredentialSet.LockedOutUser => (TestSettings.LockedOutUsername, TestSettings.Password),
                LoginCredentialSet.StandardUserWithWrongPassword => (TestSettings.StandardUsername, WrongPassword),
                _ => throw new ArgumentOutOfRangeException(nameof(credentialSet))
            };

            await _loginPage.LoginAsync(username, password);

            switch (credentialSet)
            {
                case LoginCredentialSet.StandardUser:
                    await Expect(Page).ToHaveURLAsync(new Regex(_inventoryPage.UrlPattern));
                    await Expect(_inventoryPage.PageTitle).ToHaveTextAsync("Products");
                    break;
                case LoginCredentialSet.LockedOutUser:
                    await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(LockedOutError);
                    await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
                    break;
                case LoginCredentialSet.StandardUserWithWrongPassword:
                    await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(InvalidCredentialsError);
                    await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
                    break;
            }
        }

        // -----------------------------------------------------------------------------------------
        // Logout (TestCases/Logout.md)
        // -----------------------------------------------------------------------------------------

        [Test]
        [Description("TC-LOGOUT-001: Logged-in user can log out successfully")]
        public async Task Logout_FromInventoryPage_ReturnsToLoginPage()
        {
            await _loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);
            await Expect(Page).ToHaveURLAsync(new Regex(_inventoryPage.UrlPattern));

            await _inventoryPage.LogoutAsync();

            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
            await Expect(_loginPage.UsernameInput).ToBeVisibleAsync();
            await Expect(_loginPage.PasswordInput).ToBeVisibleAsync();
            await Expect(_loginPage.LoginButton).ToBeVisibleAsync();

            // Session is cleared: reloading the previous inventory URL no longer shows authenticated content.
            await Page.GotoAsync(InventoryUrl);
            await Expect(_inventoryPage.InventoryContainer).Not.ToBeVisibleAsync();
            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(ProtectedPageError);
        }

        [Test]
        [Description("TC-LOGOUT-002: Protected inventory page is not accessible after logout via back navigation or direct URL")]
        [TestCase(PostLogoutNavigation.BrowserBack)]
        [TestCase(PostLogoutNavigation.DirectUrl)]
        public async Task ProtectedPage_AfterLogout_IsNotAccessible(PostLogoutNavigation navigation)
        {
            await _loginPage.LoginAsync(TestSettings.StandardUsername, TestSettings.Password);
            await Expect(Page).ToHaveURLAsync(new Regex(_inventoryPage.UrlPattern));

            await _inventoryPage.LogoutAsync();
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");

            if (navigation == PostLogoutNavigation.BrowserBack)
            {
                await Page.GoBackAsync();
            }
            else
            {
                await Page.GotoAsync(InventoryUrl);
            }

            // The app blocks the protected page outright: it briefly renders /inventory.html, then
            // its client-side route guard redirects back to the root login URL, showing the login
            // form again along with the same protected-page error banner used for invalid logins.
            await Expect(Page).ToHaveURLAsync(_loginPage.Url + "/");
            await Expect(_inventoryPage.InventoryContainer).Not.ToBeVisibleAsync();
            await Expect(_loginPage.LoginButton).ToBeVisibleAsync();
            await Expect(_loginPage.ErrorMessage).ToHaveTextAsync(ProtectedPageError);
        }
    }
}
