namespace SauceAppTests.Common
{
    /// <summary>
    /// Post-logout navigation methods used to drive ProtectedPage_AfterLogout_IsNotAccessible
    /// (TC-LOGOUT-002) in SauceAppTests.Tests.AuthenticationTests.
    /// </summary>
    public enum PostLogoutNavigation
    {
        BrowserBack,
        DirectUrl
    }
}
